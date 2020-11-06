using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Anno.CronNET
{
    public interface ICronSchedule
    {
        bool isValid(string expression);
        bool isTime(DateTime date_time);
    }

    public class CronSchedule : ICronSchedule
    {
        #region Readonly Class Members

        readonly static Regex divided_regex = new Regex(@"(\*/\d+)");
        readonly static Regex range_regex = new Regex(@"(\d+\-\d+)\/?(\d+)?");
        readonly static Regex wild_regex = new Regex(@"(\*)");
        readonly static Regex list_regex = new Regex(@"(((\d+,)*\d+)+)");
        readonly static Regex validation_regex = new Regex(divided_regex + "|" + range_regex + "|" + wild_regex + "|" + list_regex);

        #endregion

        #region Private Instance Members

        private readonly string _expression;
        public List<int> seconds;
        public List<int> minutes;
        public List<int> hours;
        public List<int> days_of_month;
        public List<int> months;
        public List<int> days_of_week;

        #endregion

        #region Public Constructors

        public CronSchedule()
        {
        }

        public CronSchedule(string expressions)
        {
            this._expression = expressions;
            generate();
        }

        #endregion

        #region Public Methods

        private bool isValid()
        {
            return isValid(this._expression);
        }

        public bool isValid(string expression)
        {
            MatchCollection matches = validation_regex.Matches(expression);
            return matches.Count > 0;//== 5;
        }

        public bool isTime(DateTime date_time)
        {
            return seconds.Contains(date_time.Second) && minutes.Contains(date_time.Minute) &&
                   hours.Contains(date_time.Hour) &&
                   days_of_month.Contains(date_time.Day) &&
                   months.Contains(date_time.Month) &&
                   days_of_week.Contains((int)date_time.DayOfWeek);
        }

        private void generate()
        {
            if (!isValid()) return;

            MatchCollection matches = validation_regex.Matches(this._expression);
            generate_seconds(matches[0].ToString());

            if (matches.Count > 1)
                generate_minutes(matches[1].ToString());
            else
                generate_minutes("*");
            if (matches.Count > 2)
                generate_hours(matches[2].ToString());
            else
                generate_hours("*");

            if (matches.Count > 3)
                generate_days_of_month(matches[3].ToString());
            else
                generate_days_of_month("*");

            if (matches.Count > 4)
                generate_months(matches[4].ToString());
            else
                generate_months("*");

            if (matches.Count > 5)
                generate_days_of_weeks(matches[5].ToString());
            else
                generate_days_of_weeks("*");
        }
        private void generate_seconds(string match)
        {
            this.seconds = generate_values(match, 0, 60);
        }
        private void generate_minutes(string match)
        {
            this.minutes = generate_values(match, 0, 60);
        }

        private void generate_hours(string match)
        {
            this.hours = generate_values(match, 0, 24);
        }

        private void generate_days_of_month(string match)
        {
            this.days_of_month = generate_values(match, 1, 32);
        }

        private void generate_months(string match)
        {
            this.months = generate_values(match, 1, 13);
        }

        private void generate_days_of_weeks(string match)
        {
            this.days_of_week = generate_values(match, 0, 7);
        }

        private List<int> generate_values(string configuration, int start, int max)
        {
            if (divided_regex.IsMatch(configuration)) return divided_array(configuration, start, max);
            if (range_regex.IsMatch(configuration)) return range_array(configuration);
            if (wild_regex.IsMatch(configuration)) return wild_array(configuration, start, max);
            if (list_regex.IsMatch(configuration)) return list_array(configuration);

            return new List<int>();
        }

        private List<int> divided_array(string configuration, int start, int max)
        {
            if (!divided_regex.IsMatch(configuration))
                return new List<int>();

            List<int> ret = new List<int>();
            string[] split = configuration.Split("/".ToCharArray());
            int divisor = int.Parse(split[1]);

            for (int i = start; i < max; ++i)
                if (i % divisor == 0)
                    ret.Add(i);

            return ret;
        }

        private List<int> range_array(string configuration)
        {
            if (!range_regex.IsMatch(configuration))
                return new List<int>();

            List<int> ret = new List<int>();
            string[] split = configuration.Split("-".ToCharArray());
            int start = int.Parse(split[0]);
            int end = 0;
            if (split[1].Contains("/"))
            {
                split = split[1].Split("/".ToCharArray());
                end = int.Parse(split[0]);
                int divisor = int.Parse(split[1]);

                for (int i = start; i < end; ++i)
                    if (i % divisor == 0)
                        ret.Add(i);
                return ret;
            }
            else
                end = int.Parse(split[1]);

            for (int i = start; i <= end; ++i)
                ret.Add(i);

            return ret;
        }

        private List<int> wild_array(string configuration, int start, int max)
        {
            if (!wild_regex.IsMatch(configuration))
                return new List<int>();

            List<int> ret = new List<int>();

            for (int i = start; i < max; ++i)
                ret.Add(i);

            return ret;
        }

        private List<int> list_array(string configuration)
        {
            if (!list_regex.IsMatch(configuration))
                return new List<int>();

            List<int> ret = new List<int>();

            string[] split = configuration.Split(",".ToCharArray());

            foreach (string s in split)
                ret.Add(int.Parse(s));

            return ret;
        }

        #endregion
    }
}
