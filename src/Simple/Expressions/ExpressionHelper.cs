﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Simple.Reflection;
using System.Globalization;
using Simple.Expressions;

namespace Simple
{
    public static class ExpressionHelper
    {

        public static string GetMemberName<T, P>(this Expression<Func<T, P>> expr)
        {
            return GetMemberName((LambdaExpression)expr);
        }

        public static string GetMemberName(this LambdaExpression expr)
        {
            return GetMemberPath(expr).JoinProperty();
        }

        public static IEnumerable<string> GetMemberPath<T, P>(this Expression<Func<T, P>> expr)
        {
            return GetMemberPath((LambdaExpression)expr);
        }

        public static IEnumerable<string> GetMemberPath(this LambdaExpression expr)
        {
            return expr.GetMemberList().Select(x => x.Name);
        }



        public static IEnumerable<ISettableMemberInfo> GetMemberList(this LambdaExpression expr)
        {
            if (expr != null)
                return GetMemberList(expr.Body, expr.Parameters);
            else
                return new ISettableMemberInfo[0];

        }

        public static IEnumerable<ISettableMemberInfo> GetMemberList(this Expression expr, IEnumerable<ParameterExpression> args)
        {
            LinkedList<ISettableMemberInfo> answer = new LinkedList<ISettableMemberInfo>();
            while (expr != null &&
                  (expr.NodeType == ExpressionType.MemberAccess ||
                  expr.NodeType == ExpressionType.Call ||
                  expr.NodeType == ExpressionType.Convert))
            {
                if (ExpressionType.MemberAccess == expr.NodeType)
                {
                    var memberExpr = expr as MemberExpression;
                    answer.AddFirst(memberExpr.Member.ToSettable());
                    expr = memberExpr.Expression;
                }
                else if (ExpressionType.Convert == expr.NodeType)
                {
                    expr = (expr as UnaryExpression).Operand;
                }
                else
                    throw new NotSupportedException("Not supported node type: {0}".AsFormat(expr.NodeType));
            }

            return answer;
        }

        public static IEnumerable<ISettableMemberInfo> GetMemberList(this IEnumerable<string> path, Type type)
        {
            ISettableMemberInfo ret = null;
            foreach (var prop in path)
            {
                ret = type.GetMember(prop).FirstOrDefault().ToSettable();
                if (ret == null) throw new ArgumentException("the specified property {0} doesn't exist".AsFormat(prop));
                yield return ret;

                type = ret.Type;
            }
        }

        public static ISettableMemberInfo GetMember<T>(this string path)
        {
            return GetMember(path, typeof(T));
        }

        public static ISettableMemberInfo GetMember<T>(this IEnumerable<string> path)
        {
            return GetMember(path, typeof(T));
        }

        public static ISettableMemberInfo GetMember(this string path, Type type)
        {
            return GetMember(path.SplitProperty(), type);
        }

        public static ISettableMemberInfo GetMember(this IEnumerable<string> path, Type type)
        {
            return path.GetMemberList(type).LastOrDefault();
        }



        public static Expression GetMemberExpression(this string propertyPath, Expression expr)
        {
            return propertyPath.SplitProperty().GetMemberExpression(expr);
        }


        public static Expression GetMemberExpression(this IEnumerable<string> propertyPath, Expression expr)
        {
            Expression ret = expr;

            foreach (var prop in propertyPath)
                ret = Expression.Property(ret, prop);

            return ret;
        }

        public static Expression<Func<T, object>> GetMemberExpression<T>(this string propertyPath)
        {
            return propertyPath.SplitProperty().GetMemberExpression<T>();
        }

        public static Expression<Func<T, object>> GetMemberExpression<T>(this IEnumerable<string> propertyPath)
        {
            return propertyPath.GetMemberExpression<T, object>();
        }

        public static Expression<Func<T, TProp>> GetMemberExpression<T, TProp>(this string propertyPath)
        {
            return propertyPath.SplitProperty().GetMemberExpression<T, TProp>();
        }

        public static Expression<Func<T, TProp>> GetMemberExpression<T, TProp>(this IEnumerable<string> propertyPath)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            return Expression.Lambda<Func<T, TProp>>(propertyPath.GetMemberExpression(parameter), parameter);
        }

        public static void SetValue(this LambdaExpression expr, object target, object value)
        {
            var list = expr.GetMemberList();
            var prop = list.ToSettable();

            if (value != null && !prop.Type.IsAssignableFrom(value.GetType()))
            {
                if (value.GetType().CanAssign(typeof(IConvertible)))
                    value = Convert.ChangeType(value, prop.Type.GetValueTypeIfNullable(), CultureInfo.InvariantCulture);
                else
                    throw new ArgumentException(string.Format("Don't know how to convert from {0} to {1}",
                        value.GetType().GetRealClassName(),
                        prop.Type.GetRealClassName()));
            }

            prop.Set(target, value);
        }
    }
}
