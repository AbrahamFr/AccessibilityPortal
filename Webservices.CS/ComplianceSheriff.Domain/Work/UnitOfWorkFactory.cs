using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Work
{
    class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IServiceProvider serviceProvider;

        public UnitOfWorkFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IUnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWork(serviceProvider);
        }
    }
}
