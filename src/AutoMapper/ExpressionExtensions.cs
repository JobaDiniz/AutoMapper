using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
    using static Expression;

    internal static class ExpressionExtensions
    {
        public static LambdaExpression Lambda(this IEnumerable<MemberInfo> members)
        {
            var source = Parameter(members.First().DeclaringType, "source");
            return Expression.Lambda(members.MemberAccesses(source), source);
        }
        
        public static Expression MemberAccesses(this IEnumerable<MemberInfo> members, Expression obj) =>
            members
                .Aggregate(
                        obj,
                        (inner, getter) => getter is MethodInfo method ?
                            (getter.IsStatic() ? Call(null, method, inner) : (Expression)Call(inner, method)) :
                            MakeMemberAccess(getter.IsStatic() ? null : inner, getter));

        public static IEnumerable<MemberInfo> GetMembersChain(this LambdaExpression lambda) => lambda.Body.GetMembersChain();

        public static IEnumerable<MemberInfo> GetMembersChain(this Expression expression)
        {
            return GetMembersCore().Reverse();
            IEnumerable<MemberInfo> GetMembersCore()
            {
                while (expression != null)
                {
                    if (expression is MemberExpression member)
                    {
                        yield return member.Member;
                        expression = member.Expression;
                    }
                    else if (expression is MethodCallExpression method)
                    {
                        yield return method.Method;
                        expression = method.Arguments[0];
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public static IEnumerable<MemberExpression> GetMemberExpressions(this Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if(memberExpression == null)
            {
                return new MemberExpression[0];
            }
            return memberExpression.GetMembersExpressions();
        }

        public static IEnumerable<MemberExpression> GetMembersExpressions(this MemberExpression expression)
        {
            return GetMembersCore().Reverse();
            IEnumerable<MemberExpression> GetMembersCore()
            {
                while (expression != null)
                {
                    yield return expression;
                    expression = expression.Expression as MemberExpression;
                }
            }
        }

        public static void EnsureMemberPath(this LambdaExpression exp, string name)
        {
            if(!exp.IsMemberPath())
            {
                throw new ArgumentOutOfRangeException(name, "Only member accesses are allowed. "+exp);
            }
        }

        public static bool IsMemberPath(this LambdaExpression exp) => exp.Body.GetMemberExpressions().FirstOrDefault()?.Expression == exp.Parameters.First();

        public static Expression ReplaceParameters(this LambdaExpression exp, params Expression[] replace)
            => ExpressionFactory.ReplaceParameters(exp, replace);

        public static Expression ConvertReplaceParameters(this LambdaExpression exp, params Expression[] replace)
            => ExpressionFactory.ConvertReplaceParameters(exp, replace);

        public static Expression Replace(this Expression exp, Expression old, Expression replace)
            => ExpressionFactory.Replace(exp, old, replace);

        public static LambdaExpression Concat(this LambdaExpression expr, LambdaExpression concat)
            => ExpressionFactory.Concat(expr, concat);

        public static Expression NullCheck(this Expression expression, Type destinationType = null)
            => ExpressionFactory.NullCheck(expression, destinationType);

        public static Expression IfNullElse(this Expression expression, Expression then, Expression @else = null)
            => ExpressionFactory.IfNullElse(expression, then, @else);
    }
}