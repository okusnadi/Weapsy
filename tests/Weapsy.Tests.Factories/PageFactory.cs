﻿using System;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Weapsy.Domain.Model.Pages;
using Weapsy.Domain.Model.Pages.Commands;
using System.Collections.Generic;

namespace Weapsy.Tests.Factories
{
    public static class PageFactory
    {
        public static Page Page()
        {
            return Page(Guid.NewGuid(), Guid.NewGuid(), "My Page");
        }

        public static Page Page(Guid siteId, Guid id, string name)
        {
            var createCommand = new CreatePage
            {
                SiteId = siteId,
                Id = id,
                Name = name,
                Url = "url",
                Title = "Title",
                MetaDescription = "Meta Description",
                MetaKeywords = "Meta Keywords",
                PageLocalisations = new List<PageDetails.PageLocalisation>
                {
                    new PageDetails.PageLocalisation
                    {
                        LanguageId = Guid.NewGuid(),
                        Url = "url",
                        Title = "Head Title",
                        MetaDescription = "Meta Description",
                        MetaKeywords = "Meta Keywords"
                    }
                }
            };

            var createValidatorMock = new Mock<IValidator<CreatePage>>();
            createValidatorMock.Setup(x => x.Validate(createCommand)).Returns(new ValidationResult());

            var page = Domain.Model.Pages.Page.CreateNew(createCommand, createValidatorMock.Object);

            var addModuleCommand = new AddPageModule
            {
                SiteId = siteId,
                PageId = id,
                ModuleId = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                Title = "Title",
                Zone = "Zone",
                SortOrder = 1
            };

            var addModuleValidatorMock = new Mock<IValidator<AddPageModule>>();
            addModuleValidatorMock.Setup(x => x.Validate(addModuleCommand)).Returns(new ValidationResult());

            page.AddModule(addModuleCommand, addModuleValidatorMock.Object);

            return page;
        }
    }
}
