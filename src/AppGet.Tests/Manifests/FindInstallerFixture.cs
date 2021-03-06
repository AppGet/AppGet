﻿using System.Collections.Generic;
using AppGet.Installers.Requirements;
using AppGet.Manifest;
using AppGet.Manifests;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AppGet.Tests.Manifests
{
    [TestFixture]
    public class FindInstallerFixture : TestBase<FindInstaller>
    {
        private Mock<IEnforceRequirements> _approved;
        private Mock<IEnforceRequirements> _rejected;

        private Installer _x86Installer;
        private Installer _x64Installer;
        private List<Installer> _installers;

        [SetUp]
        public void Setup()
        {
            _approved = new Mock<IEnforceRequirements>();
            _rejected = new Mock<IEnforceRequirements>();

            _approved.Setup(s => s.IsRequirementSatisfied(It.IsAny<Installer>()))
                     .Returns(EnforcementResult.Pass);

            _rejected.Setup(s => s.IsRequirementSatisfied(It.IsAny<Installer>()))
                     .Returns(EnforcementResult.Fail("Test failure"));

            _x86Installer = new Installer { Architecture = ArchitectureTypes.x86 };
            _x64Installer = new Installer { Architecture = ArchitectureTypes.x64 };
        }

        private void GivenApprovedOnly()
        {
            Mocker.Use((IEnumerable<IEnforceRequirements>)new List<IEnforceRequirements> { _approved.Object });
        }

        private void GivenRejectedOnly()
        {
            Mocker.Use((IEnumerable<IEnforceRequirements>)new List<IEnforceRequirements> { _rejected.Object });
        }

        private void GivenInstallers(params Installer[] installers)
        {
            _installers = new List<Installer>(installers);
        }

        [Test]
        public void should_return_null_if_no_suitable_installer_is_found()
        {
            GivenRejectedOnly();
            GivenInstallers(_x64Installer);
            Assert.Throws<PackageNotCompatibleException>(() => Subject.GetBestInstaller(_installers));
        }

        [Test]
        public void should_prefer_x64_over_x86()
        {
            GivenApprovedOnly();
            GivenInstallers(_x86Installer, _x64Installer);

            Subject.GetBestInstaller(_installers).Should().Be(_x64Installer);
        }

    }
}
