﻿using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BlazorTable
{
    public partial class Table<TableItem> : ITable<TableItem>
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public IDictionary<string, object> UnknownParameters { get; set; }

        [Parameter]
        public string TableClass { get; set; } = "table table-striped table-bordered table-hover table-sm";

        [Parameter]
        public string TableHeadClass { get; set; } = "thead-light text-dark";

        [Parameter]
        public string TableBodyClass { get; set; } = "";

        [Parameter]
        public Expression<Func<TableItem, string>> TableRowClass { get; set; }

        [Parameter]
        public int PageSize { get; set; }

        [Parameter]
        public bool ColumnReorder { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public IEnumerable<TableItem> Items { get; set; }

        [Inject]
        private ILogger<ITable<TableItem>> Logger { get; set; }

        private IEnumerable<TableItem> TempItems { get; set; }

        public List<IColumn<TableItem>> Columns { get; } = new List<IColumn<TableItem>>();

        public int PageNumber { get; private set; }

        public int TotalCount { get; private set; }

        public bool IsEditMode { get; private set; }

        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        protected override void OnParametersSet()
        {
            Update();
        }

        private IEnumerable<TableItem> GetData()
        {
            if (Items != null)
            {
                var query = Items.AsQueryable();

                foreach (var item in Columns)
                {
                    if (item.Filter != null)
                    {
                        query = query.Where(item.Filter);
                    }
                }

                TotalCount = query.Count();

                var sortColumn = Columns.Find(x => x.SortColumn);

                if (sortColumn != null)
                {
                    if (sortColumn.SortDescending)
                    {
                        query = query.OrderByDescending(sortColumn.Field);
                    }
                    else
                    {
                        query = query.OrderBy(sortColumn.Field);
                    }
                }

                return query.Skip(PageNumber * PageSize).Take(PageSize).ToList();
            }

            return Items;
        }

        public void Update()
        {
            TempItems = GetData();
            Refresh();
        }

        public void AddColumn(IColumn<TableItem> column)
        {
            Columns.Add(column);
            Refresh();
        }

        public void RemoveColumn(IColumn<TableItem> column)
        {
            Columns.Remove(column);
            Refresh();
        }

        public void FirstPage()
        {
            if (PageNumber != 0)
            {
                PageNumber = 0;
                Update();
            }
        }

        public void NextPage()
        {
            if (PageNumber + 1 < TotalPages)
            {
                PageNumber++;
                Update();
            }
        }

        public void PreviousPage()
        {
            if (PageNumber > 0)
            {
                PageNumber--;
                Update();
            }
        }

        public void LastPage()
        {
            PageNumber = TotalPages - 1;
            Update();
        }

        public void ToggleEditMode()
        {
            IsEditMode = !IsEditMode;
            StateHasChanged();
        }

        public void Refresh()
        {
            StateHasChanged();
        }

        private IColumn<TableItem> DragSource;

        private void HandleDragStart(IColumn<TableItem> column)
        {
            DragSource = column;
        }

        private void HandleDrop(IColumn<TableItem> column)
        {
            int index = Columns.FindIndex(a => a == column);

            Columns.Remove(DragSource);

            Columns.Insert(index, DragSource);

            StateHasChanged();
        }

        /// <summary>
        /// Return row class for item if expression is specified
        /// </summary>
        /// <param name="item">TableItem to return for</param>
        /// <returns></returns>
        private string RowClass(TableItem item)
        {
            if (TableRowClass == null) return null;
            var expr = TableRowClass.Compile();
            return expr.Invoke(item);
        }
    }
}
