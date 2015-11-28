using System;
using System.Collections.Generic;
using NEdifis.Conventions;
using NUnit.Framework;

namespace NuPlug
{
    internal class NuPlugConventions : ConventionBase
    {
        private static readonly IEnumerable<Type> TypesToTest = ClassesToTestFor<NuPlugConventions>();

        public NuPlugConventions()
        {
            // NEDifis built-in
            Conventions.AddRange(new IVerifyConvention[]
            {
                new ExcludeFromCodeCoverageClassHasBecauseAttribute(),
                new AllClassesNeedATest(),
                new TestClassesShouldMatchClassToTest(),
                new TestClassesShouldBePrivate(),
            });
            // customized
            Conventions.AddRange(ConventionsFor<NuPlugConventions>());
        }

        [Test, TestCaseSource(nameof(TypesToTest))]
        public void Check(Type typeToTest)
        {
            Conventions.Check(typeToTest);
        }
    }
}