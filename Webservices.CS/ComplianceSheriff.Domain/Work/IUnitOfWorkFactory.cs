using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Work
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork CreateUnitOfWork();
    }
}
