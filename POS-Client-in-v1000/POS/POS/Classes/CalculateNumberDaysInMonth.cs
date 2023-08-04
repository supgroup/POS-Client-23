﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Classes
{
    public class CalculateNumberDaysInMonth
    {
        public decimal percentValue(decimal? value, decimal? percent)
        {
            decimal? perval = (value * percent / 100);
            return (decimal)perval;
        }
        public DateTime? changeDateformat(DateTime? date, string format)
        {//@"d/M/yyyy"
            string sdate = "";
            if (date != null)
            {
                DateTime ts = DateTime.Parse(date.ToString());
                // @"hh\:mm\:ss"
                sdate = ts.ToString(format);
            }

            return DateTime.Parse(sdate);
        }
        public int getdays(DateTime date)
        {
            int year;
            int month;
            int days;

            year = date.Year;
            month = date.Month;

            days = getdays(year, month);



            //  int days = DateTime.DaysInMonth(year, month);

            return days;
        }
        public int getdays(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);

            return days;
        }
    }
}
