using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Glow.Core.Views;
using Glow.TypeScript;
using Microsoft.AspNetCore.Mvc;

namespace Glow.Sample.Views
{
    [GenerateTsInterface]
    public class ListViewItem
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public DateTime Birthday { get; set; }
        public string City { get; set; }
    }

    [Route("api/list-view")]
    public class SampleListView : ListView<ListViewItem>
    {
        private static readonly List<ListViewItem> data = new Faker<ListViewItem>()
            .UseSeed(1)
            .RuleFor(v => v.Id, f => Guid.NewGuid())
            .RuleFor(v => v.DisplayName, f => f.Person.UserName)
            .RuleFor(v => v.Birthday, f => f.Person.DateOfBirth)
            .RuleFor(v => v.City, f => f.Address.City())
            .Generate(1000);

        protected override IQueryable<ListViewItem> Get(string search = "")
        {
            IQueryable<ListViewItem> q = data.AsQueryable();
            if (string.IsNullOrEmpty(search))
            {
                return q;
            }
            else
            {
                return q.Where(v => v.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) || v.City.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
