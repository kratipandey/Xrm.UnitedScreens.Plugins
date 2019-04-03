using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Xrm.US.Plugins
{
    internal class EntityMetadata<T>
    {
        private static PropertyInfo GetAttributeProperty(Expression<Func<T, object>> columnExpression)
        {
            var bodyExpression = columnExpression.Body;

            if (bodyExpression is UnaryExpression && bodyExpression.NodeType == ExpressionType.Convert)
                bodyExpression = ((UnaryExpression)bodyExpression).Operand;

            var memberExpr = bodyExpression as System.Linq.Expressions.MemberExpression;
            string memberName = memberExpr.Member.Name;

            var member = typeof(T).GetProperty(memberName);

            return member;
        }

        public static string AttributeName(Expression<Func<T, object>> columnExpression)
        {
            var member = GetAttributeProperty(columnExpression);

            var mcm = member.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), true).Cast<AttributeLogicalNameAttribute>().Single();
            return mcm.LogicalName;
        }

        
    }

   


}
