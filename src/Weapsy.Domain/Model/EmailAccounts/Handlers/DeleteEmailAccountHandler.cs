using FluentValidation;
using System;
using System.Collections.Generic;
using Weapsy.Core.Domain;
using Weapsy.Domain.Model.EmailAccounts.Commands;

namespace Weapsy.Domain.Model.EmailAccounts.Handlers
{
    public class DeleteEmailAccountHandler : ICommandHandler<DeleteEmailAccount>
    {
        private readonly IEmailAccountRepository _emailAccountRepository;
        private readonly IValidator<DeleteEmailAccount> _validator;

        public DeleteEmailAccountHandler(IEmailAccountRepository emailAccountRepository, IValidator<DeleteEmailAccount> validator)
        {
            _emailAccountRepository = emailAccountRepository;
            _validator = validator;
        }

        public ICollection<IEvent> Handle(DeleteEmailAccount command)
        {
            var emailAccount = _emailAccountRepository.GetById(command.SiteId, command.Id);

            if (emailAccount == null)
                throw new Exception("Email Account not found.");

            emailAccount.Delete(command, _validator);

            _emailAccountRepository.Update(emailAccount);

            return emailAccount.Events;
        }
    }
}
