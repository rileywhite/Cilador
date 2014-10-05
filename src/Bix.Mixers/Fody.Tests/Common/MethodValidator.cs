using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Tests.Common
{
    /// <summary>
    /// Provides a way to complete some validation of a cloned method against its source method.
    /// </summary>
    public static class MethodValidator
    {
        /// <summary>
        /// Validates an actual method (the cloned version) against an expected method (its source).
        /// </summary>
        /// <param name="actualMethod">Method to validate.</param>
        /// <param name="expectedMethod">Method with expected info.</param>
        public static void ValidateMethod(MethodInfo actualMethod, MethodInfo expectedMethod)
        {
            Assert.That(actualMethod == null, Is.EqualTo(expectedMethod == null));
            if (actualMethod == null) { return; }

            Assert.That(actualMethod.Name, Is.EqualTo(expectedMethod.Name));

            // TODO more things?

            ValidateMethodBody(actualMethod.GetMethodBody(), expectedMethod.GetMethodBody());
        }

        /// <summary>
        /// Validates an actual method body (the cloned version) against an expected method body (its source).
        /// </summary>
        /// <param name="actualMethodBody">Method body to validate.</param>
        /// <param name="expectedMethodBody">Method body with expeted info.</param>
        private static void ValidateMethodBody(MethodBody actualMethodBody, MethodBody expectedMethodBody)
        {
            Assert.That(actualMethodBody == null, Is.EqualTo(expectedMethodBody == null));
            if (actualMethodBody == null) { return; }

            Assert.That(actualMethodBody.InitLocals, Is.EqualTo(expectedMethodBody.InitLocals));
            Assert.That(actualMethodBody.MaxStackSize, Is.EqualTo(expectedMethodBody.MaxStackSize));

            Assert.That(actualMethodBody.ExceptionHandlingClauses.Count, Is.EqualTo(expectedMethodBody.ExceptionHandlingClauses.Count));
            for (int i = 0; i < actualMethodBody.ExceptionHandlingClauses.Count; i++)
            {
                ValidateExceptionHandlingClause(actualMethodBody.ExceptionHandlingClauses[i], expectedMethodBody.ExceptionHandlingClauses[i]);
            }

            Assert.That(actualMethodBody.LocalVariables.Count, Is.EqualTo(expectedMethodBody.LocalVariables.Count));
            for (int i = 0; i < actualMethodBody.LocalVariables.Count; i++)
            {
                ValidateLocalVariable(actualMethodBody.LocalVariables[i], expectedMethodBody.LocalVariables[i]);
            }
        }

        /// <summary>
        /// Validates an actual exception handling clause (the cloned version) against an expected one (its source).
        /// </summary>
        /// <param name="actualExceptionHandlingClause">Exception handling clause to validate.</param>
        /// <param name="expectedExceptionHandlingClause">Exception handling clause with expected info.</param>
        private static void ValidateExceptionHandlingClause(
            ExceptionHandlingClause actualExceptionHandlingClause,
            ExceptionHandlingClause expectedExceptionHandlingClause)
        {
            throw new NotImplementedException("TODO implement exception handling clause validation");
        }

        /// <summary>
        /// Validates an actual exception handling clause (the cloned version) against an expected one (its source).
        /// </summary>
        /// <param name="actualLocalVariable">Local variable to validate.</param>
        /// <param name="expectedLocalVariable">Local variable with expected info.</param>
        private static void ValidateLocalVariable(LocalVariableInfo actualLocalVariable, LocalVariableInfo expectedLocalVariable)
        {
            Assert.That(actualLocalVariable == null, Is.EqualTo(expectedLocalVariable == null));
            if (actualLocalVariable == null) { return; }

            Assert.That(actualLocalVariable.IsPinned, Is.EqualTo(expectedLocalVariable.IsPinned));
            Assert.That(actualLocalVariable.LocalIndex, Is.EqualTo(expectedLocalVariable.LocalIndex));
        }
    }
}
