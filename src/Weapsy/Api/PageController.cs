﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Weapsy.Core.Dispatcher;
using Weapsy.Domain.Model.Pages;
using Weapsy.Domain.Model.Pages.Commands;
using Weapsy.Domain.Model.Pages.Rules;
using Weapsy.Domain.Services.Modules.Commands;
using Weapsy.Mvc.Context;
using Weapsy.Mvc.Controllers;
using Weapsy.Reporting.Pages;

namespace Weapsy.Api
{
    [Route("api/[controller]")]
    public class PageController : BaseAdminController
    {
        private readonly IPageFacade _pageFacade;
        private readonly ICommandSender _commandSender;
        private readonly IPageRules _pageRules;
        private readonly IPageRepository _pageRepository;

        public PageController(IPageFacade pageFacade,
            ICommandSender commandSender,
            IPageRules pageRules,
            IPageRepository pageRepository,
            IContextService contextService)
            : base(contextService)
        {
            _pageFacade = pageFacade;
            _commandSender = commandSender;
            _pageRules = pageRules;
            _pageRepository = pageRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var pages = await Task.Run(() => _pageRepository.GetAll(SiteId));
            return Ok(pages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var page = await Task.Run(() => _pageRepository.GetById(SiteId, id));
            if (page == null) return NotFound();
            return Ok(page);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreatePage model)
        {
            model.SiteId = SiteId;
            model.Id = Guid.NewGuid();
            await Task.Run(() => _commandSender.Send<CreatePage, Page>(model));
            return Ok(string.Empty);
        }

        [HttpPut]
        [Route("{id}/update")]
        public async Task<IActionResult> UpdateDetails([FromBody] UpdatePageDetails model)
        {
            model.SiteId = SiteId;
            await Task.Run(() => _commandSender.Send<UpdatePageDetails, Page>(model));
            return Ok(string.Empty);
        }

        [HttpPut]
        [Route("{id}/add-module")]
        public async Task<IActionResult> AddModule([FromBody] AddModule model)
        {
            model.SiteId = SiteId;
            await Task.Run(() => _commandSender.Send<AddModule, Page>(model));
            return Ok(string.Empty);
        }

        [HttpPut]
        [Route("{id}/remove-module")]
        public async Task<IActionResult> RemoveModule([FromBody] RemoveModule model)
        {
            model.SiteId = SiteId;
            await Task.Run(() => _commandSender.Send<RemoveModule, Page>(model));
            return Ok(string.Empty);
        }

        [HttpPut]
        [Route("{id}/reorder-modules")]
        public async Task<IActionResult> ReorderPageModules([FromBody] ReorderPageModules model)
        {
            model.SiteId = SiteId;
            await Task.Run(() => _commandSender.Send<ReorderPageModules, Page>(model));
            return Ok(string.Empty);
        }

        [HttpPut]
        [Route("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            await Task.Run(() => _commandSender.Send<ActivatePage, Page>(new ActivatePage
            {
                SiteId = SiteId,
                Id = id
            }));
            return Ok(string.Empty);
        }

        [HttpPut]
        [Route("{id}/hide")]
        public async Task<IActionResult> Hide(Guid id)
        {
            await Task.Run(() => _commandSender.Send<HidePage, Page>(new HidePage
            {
                SiteId = SiteId,
                Id = id
            }));
            return Ok(string.Empty);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Task.Run(() => _commandSender.Send<DeletePage, Page>(new DeletePage
            {
                SiteId = SiteId,
                Id = id
            }));
            return Ok(string.Empty);
        }

        [HttpGet("{name}")]
        [Route("isPageNameUnique")]
        public IActionResult IsPageNameUnique(string name)
        {
            var isPageNameUnique = _pageRules.IsPageNameUnique(SiteId, name);
            return Ok(isPageNameUnique);
        }

        [HttpGet("{name}")]
        [Route("isPageNameValid")]
        public IActionResult IsPageNameValid(string name)
        {
            var isPageNameValid = _pageRules.IsPageNameValid(name);
            return Ok(isPageNameValid);
        }

        [HttpGet("{url}")]
        [Route("isPageUrlUnique")]
        public IActionResult IsPageUrlUnique(string url)
        {
            var isPageUrlUnique = _pageRules.IsPageUrlUnique(SiteId, url);
            return Ok(isPageUrlUnique);
        }

        [HttpGet("{url}")]
        [Route("isPageUrlValid")]
        public IActionResult IsPageUrlValid(string url)
        {
            var isPageUrlValid = _pageRules.IsPageUrlValid(url);
            return Ok(isPageUrlValid);
        }

        [HttpGet("{url}")]
        [Route("isPageUrlReserved")]
        public IActionResult IsPageUrlReserved(string url)
        {
            var isPageUrlReserved = _pageRules.IsPageUrlReserved(url);
            return Ok(isPageUrlReserved);
        }

        [HttpGet]
        [Route("{id}/view")]
        public async Task<IActionResult> ViewById(Guid id)
        {
            var model = await Task.Run(() => _pageFacade.GetPageViewModel(SiteId, id));
            if (model == null) return NotFound();
            return Ok(model);
        }

        [HttpGet]
        [Route("{name}/view")]
        public async Task<IActionResult> ViewByName(string name)
        {
            var model = await Task.Run(() => _pageFacade.GetPageViewModel(SiteId, name));
            if (model == null) return NotFound();
            return Ok(model);
        }

        [HttpGet]
        [Route("{id}/admin-list")]
        public async Task<IActionResult> AdminList()
        {
            var model = await _pageFacade.GetAllForAdminAsync(SiteId);
            return Ok(model);
        }

        [HttpGet]
        [Route("{id}/admin-edit")]
        public async Task<IActionResult> AdminEdit(Guid id)
        {
            var model = await _pageFacade.GetAdminModelAsync(SiteId, id);
            if (model == null) return NotFound();
            return Ok(model);
        }
    }
}
