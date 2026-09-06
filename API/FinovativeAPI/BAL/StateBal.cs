using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.User;
using Project.Response.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class StateBal
    {
        private readonly StateDAL ObjDal = new StateDAL();

        internal List<StateViewModel> Select()
        {
            List<StateViewModel> states = new List<StateViewModel>();
            List<StateMaster> ReturnList;
            try
            {
                ReturnList = ObjDal.Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return states;
            }
            foreach (StateMaster state in ReturnList)
            {
                states.Add(new StateViewModel
                {
                    StateID = state.ID,
                    Name = state.Name
                });
            }

            return states;
        }

        internal List<StateViewModel> SelectStateForDropDown(SelectStateForDropDownRequest mainRequest)
        {
            List<StateViewModel> states = new List<StateViewModel>();
            List<StateMaster> ReturnList;
            try
            {
                ReturnList = ObjDal.Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return states;
            }
            foreach (StateMaster state in ReturnList)
            {
                states.Add(new StateViewModel
                {
                    StateID = state.ID,
                    Name = state.Name
                });
            }

            if (states == null)
                return states;

            if (states.Count <= 0)
                return states;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.Name))
                    states = states.AsQueryable().WhereDynamic(mainRequest.Name).ToList();
            }

            return states;
        }
    }
}