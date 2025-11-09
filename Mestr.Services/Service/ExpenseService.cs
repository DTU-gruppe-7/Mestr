using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mestr.Data.Interface;
using Mestr.Services.Interface;
using Mestr.Core.Model;
using Mestr.Core.Enum;
using Mestr.Data.Repository;
using Mestr.Data.Interface;

namespace Mestr.Services.Service
{
    public class ExpenseService : IExpenseService
    {
        private readonly IRepository<Expense> _expenseRepository;
        public ExpenseService(IRepository <Expense> expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }
        public Expense GetByUuid(Guid uuid)
        {
            return _expenseRepository.GetByUuid(uuid);
        }
        public IEnumerable<Expense> GetAllByProjectUuid(Guid projektUuid)
        {
            return _expenseRepository.GetAll().Where(e => e.ProjectUuid == projektUuid).ToList();
        }
        public Expense AddNewExpense(Guid projectUuid, string description, decimal amount, DateTime date, Mestr.Core.Enum.ExpenseCategory category)
        {
            var expense = new Expense(Guid.NewGuid(), projectUuid, description, amount, date, category, false);
            _expenseRepository.Add(expense);
            return expense;
        }
        public bool Delete(Expense entity)
        {
            _expenseRepository.Delete(entity.Uuid);
            return true;
        }
        public Expense Update(Expense entity)
        {
            _expenseRepository.Update(entity);
            return entity;
        }
    }
}
