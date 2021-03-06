using System;
using System.Collections.Generic;
using Weapsy.Core.Domain;
using Weapsy.Domain.Model.Templates.Commands;

namespace Weapsy.Domain.Model.Templates.Handlers
{
    public class DeleteTemplateHandler : ICommandHandler<DeleteTemplate>
    {
        private readonly ITemplateRepository _templateRepository;

        public DeleteTemplateHandler(ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public ICollection<IEvent> Handle(DeleteTemplate command)
        {
            var template = _templateRepository.GetById(command.Id);

            if (template == null)
                throw new Exception("Template not found.");

            template.Delete();

            _templateRepository.Update(template);

            return template.Events;
        }
    }
}
