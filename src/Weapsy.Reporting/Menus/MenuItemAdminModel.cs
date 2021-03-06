﻿using System;
using System.Collections.Generic;
using Weapsy.Domain.Model.Menus;

namespace Weapsy.Reporting.Menus
{
    public class MenuItemAdminModel
    {
        public Guid Id { get; set; }
        public MenuItemType MenuItemType { get; set; }
        public Guid PageId { get; set; }
        public string Link { get; set; }
        public string Text { get; set; }
        public string Title { get; set; }

        public List<MenuItemLocalisation> MenuItemLocalisations { get; set; } = new List<MenuItemLocalisation>();

        public class MenuItemLocalisation
        {
            public Guid MenuItemId { get; set; }
            public Guid LanguageId { get; set; }
            public string LanguageName { get; set; }
            public string Text { get; set; }
            public string Title { get; set; }
        }
    }
}
