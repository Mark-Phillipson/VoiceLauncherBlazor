using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Models;

namespace VoiceLauncherBlazor.Pages
{
    public partial class Examples
    {
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414

        public bool ShowDialog { get; set; }
        private int exampleIdDelete { get; set; }

        public string StatusMessage { get; set; }
        private IQueryable<VoiceLauncherBlazor.Models.Example> examples;
        protected override void OnInitialized()
        {
            LoadExamples();
        }
        private void LoadExamples()
        {
            try
            {
                examples = ExampleService.GetExamples();
            }
            catch (System.Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }

        void OnRowRemoving(Example dataItem)
        {
            exampleIdDelete = dataItem.Id;
            ShowDialog = true;
            StateHasChanged();
            return;
        }
        async Task OnRowUpdating(Example dataItem, IDictionary<string, object> newValue)
        {
            Example example = ConvertToExample(newValue, dataItem);
            example.Id = dataItem.Id;
            StatusMessage = await ExampleService.SaveExample(example);
            StateHasChanged();
        }
        async Task OnRowInserting(IDictionary<string, object> newValue)
        {
            Example example = ConvertToExample(newValue);
            StatusMessage = await ExampleService.SaveExample(example);
            StateHasChanged();
        }

        private static Example ConvertToExample(IDictionary<string, object> newValue, Example example = null)
        {
            if (example == null)
            {
                example = new Example();
            }
            foreach (var item in newValue)
            {
                if (item.Key == "DateValue")
                {
                    example.DateValue = (DateTime)item.Value;
                }
                else if (item.Key == "NumberValue")
                {
                    example.NumberValue = (int)item.Value;
                }
                else if (item.Key == "Text")
                {
                    example.Text = (string)item.Value;
                }
                else if (item.Key == "LargeText")
                {
                    example.LargeText = (string)item.Value;
                }
                else if (item.Key == "Boolean")
                {
                    example.Boolean = (bool)item.Value;
                }
            }
            return example;
        }

        Task OnInitNewRow(Dictionary<string, object> values)
        {
            values.Add("DateValue", DateTime.Now);
            values.Add("NumberValue", 13);
            values.Add("Text", "Warm");
            values.Add("LargeText", "Sunny");
            values.Add("Boolean", true);
            return Task.CompletedTask;
        }
        void CancelDelete()
        {
            ShowDialog = false;
        }
        async Task DeleteExample(int exampleId)
        {
            StatusMessage = await ExampleService.DeleteExample(exampleId);
            ShowDialog = false;
            LoadExamples();
            StateHasChanged();
        }
    }
}
