﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Valit.Helpers;
using Valit.Interfaces;

namespace Valit
{
    public class ValidationRule
    {
        public static ValidationRule<T> NewRule<T>()
        {
            return new ValidationRule<T>();
        }
    }

    public class ValidationRule<T> : IValidationRule<T>
    {
        private Func<T, bool> ValidationFunction { get; set; }

        private Func<T, string> ErrorMessageFunction { get; set; }

        private IEnumerable<string> InvalidMembers { get; set; }

        public string Description { get; private set; }

        public override string ToString()
        {
            return Description ?? base.ToString();
        }

        internal ValidationRule() { }
        
        public virtual ValidationRuleResult Validate(T instance)
        {
            if (ValidationFunction == null) { throw new ArgumentNullException(nameof(ValidationFunction)); }

            var res = new ValidationRuleResult
            {
                IsValid = ValidationFunction(instance)
            };

            if (res.IsValid) { return res; }

            res.ValidationResults =
                ErrorMessageFunction == null
                ? ValidationResultHelper.GenericResult()
                : ValidationResultHelper.NewResult(ErrorMessageFunction(instance), InvalidMembers);

            return res;
        }

        #region "Fluent Methods"

        public ValidationRule<T> ValidIf(Func<T, bool> validationFunction)
        {
            if (validationFunction == null) { throw new ArgumentNullException(nameof(validationFunction)); }

            ValidationFunction = validationFunction;

            return this;
        }

        public ValidationRule<T> SetErrorMessage(Func<T, string> errorMessageFunction)
        {
            if (errorMessageFunction == null) { throw new ArgumentNullException(nameof(errorMessageFunction)); }

            ErrorMessageFunction = errorMessageFunction;

            return this;
        }

        public ValidationRule<T> SetErrorMessage(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage)) { throw new ArgumentNullException(nameof(errorMessage)); }

            ErrorMessageFunction = instance => errorMessage;

            return this;
        }

        public ValidationRule<T> AddInvalidMember<TProp>(Expression<Func<T, TProp>> property)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            if (propertyInfo == null) { throw new ArgumentException($"The lambda expression '{nameof(property)}' should point to a valid Property"); }

            var memberList = new List<string>();

            if (InvalidMembers != null && InvalidMembers.Any()) { memberList.AddRange(InvalidMembers); }
            memberList.Add(propertyInfo.Name);

            InvalidMembers = memberList;

            return this;
        }

        public ValidationRule<T> SetDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) { throw new ArgumentNullException(nameof(description)); }

            Description = description;

            return this;
        }

        #endregion
    }
}