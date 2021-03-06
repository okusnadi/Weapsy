﻿using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Weapsy.Domain.Model.Menus;
using Weapsy.Domain.Model.Menus.Commands;

namespace Weapsy.Tests.Factories
{
    public static class MenuFactory
    {
        public static Menu Menu()
        {
            return Menu(Guid.NewGuid(), Guid.NewGuid(), "My Menu", "My Item", "My Item Localised");
        }

        public static Menu Menu(Guid siteId, Guid id, string name, string itemText, string itemTextLocalised)
        {
            var createCommand = new CreateMenu
            {
                SiteId = siteId,
                Id = id,
                Name = name
            };

            var createValidatorMock = new Mock<IValidator<CreateMenu>>();
            createValidatorMock.Setup(x => x.Validate(createCommand)).Returns(new ValidationResult());

            var menu = Domain.Model.Menus.Menu.CreateNew(createCommand, createValidatorMock.Object);

            var addItemCommand = new AddMenuItem
            {
                SiteId = menu.SiteId,
                MenuId = menu.Id,
                MenuItemId = Guid.NewGuid(),
                MenuItemType = MenuItemType.Link,
                PageId = Guid.NewGuid(),
                Link = "link",
                Text = itemText,
                Title = "Title",
                MenuItemLocalisations = new List<MenuItemDetails.MenuItemLocalisation>
                {
                    new MenuItemDetails.MenuItemLocalisation
                    {
                        LanguageId = Guid.NewGuid(),
                        Text = itemTextLocalised,
                        Title = "Title 1"
                    }
                }
            };

            var addItemValidatorMock = new Mock<IValidator<AddMenuItem>>();
            addItemValidatorMock.Setup(x => x.Validate(addItemCommand)).Returns(new ValidationResult());

            menu.AddMenuItem(addItemCommand, addItemValidatorMock.Object);

            return menu;
        }
    }
}
