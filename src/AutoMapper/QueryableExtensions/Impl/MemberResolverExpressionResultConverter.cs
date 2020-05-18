using System.Linq;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberResolverExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap, LetPropertyMaps letPropertyMaps)
        {
            var mapFrom = propertyMap.CustomMapExpression;
            if (!IsSubQuery() || letPropertyMaps.ConfigurationProvider.ResolveTypeMap(propertyMap.SourceType, propertyMap.DestinationType) == null)
            {
                var newMapFrom = mapFrom.ReplaceParameters(
                    propertyMap.CustomSource == null ?
                        expressionResolutionResult.ResolutionExpression :
                        letPropertyMaps.GetSubQueryMarker(propertyMap.CustomSource));
                return new ExpressionResolutionResult(newMapFrom);
            }
            if (propertyMap.CustomSource != null)
            {
                var marker = letPropertyMaps.GetSubQueryMarker(propertyMap.CustomSource);
                return new ExpressionResolutionResult(mapFrom.ReplaceParameters(marker));
            }
            var result = letPropertyMaps.GetSubQueryMarker(mapFrom);
            return new ExpressionResolutionResult(result);
            bool IsSubQuery()
            {
                if (!(mapFrom.Body is MethodCallExpression methodCall))
                {
                    return false;
                }
                var method = methodCall.Method;
                return method.IsStatic && method.DeclaringType == typeof(Enumerable);
            }
        }
        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap) => propertyMap.CustomMapExpression != null;
    }
}