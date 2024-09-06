using DataAccessLibrary.Models;
using DataAccessLibrary.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WindowsInput.Native;

namespace DataAccessLibrary
{
    public class CreateCommands
    {
        private readonly ApplicationDbContext _context;

        public CreateCommands(ApplicationDbContext context)
        {
            _context = context;
        }
        public void CreateCommandsFromList(string nameOfGrammar, string spokenFormPrefix)
        {
            var grammarName = _context.GrammarNames.Where(v => v.NameOfGrammar == nameOfGrammar).FirstOrDefault();
            var items = _context.GrammarItems.Where(v => grammarName != null && v.GrammarNameId == grammarName.Id).ToList();
            foreach (GrammarItem item in items)
            {
                WindowsSpeechVoiceCommand command = new();
                command.ApplicationName = "Global";
                command.Description = $"Auto created: {DateTime.Now}";
                command.AutoCreated = true;
                _context.WindowsSpeechVoiceCommands.Add(command);
                _context.SaveChanges();
                SpokenForm spokenForm = new SpokenForm();
                spokenForm.WindowsSpeechVoiceCommandId = command.Id;
                spokenForm.SpokenFormText = $"{spokenFormPrefix} {item.Value}";
                _context.SaveChanges();
                CustomWindowsSpeechCommand action = new();
                action.WindowsSpeechVoiceCommandId = command.Id;
                if (spokenFormPrefix == "Move Left")
                {
                    action.SendKeysValue = "{Left " + item.Value + "}";
                }
                else
                {
                    action.SendKeysValue = "{Right " + item.Value + "}";
                }
                //action.TextToEnter = $"{item.Value}";
                _context.CustomWindowsSpeechCommands.Add(action);
                _context.SaveChanges();
            }
        }
        public void CreateCommandSqlCommands()
        {
            var grammarName = _context.GrammarNames.Where(v => v.NameOfGrammar == "Sequel").FirstOrDefault();
            var keywordItems = _context.GrammarItems.Where(v => grammarName != null && v.GrammarNameId == grammarName.Id).ToList();
            foreach (GrammarItem item in keywordItems)
            {
                WindowsSpeechVoiceCommand command = new();
                command.ApplicationName = "Global";
                command.Description = $"Auto created: {DateTime.Now}";
                _context.WindowsSpeechVoiceCommands.Add(command);
                _context.SaveChanges();
                SpokenForm spokenForm = new SpokenForm();
                spokenForm.WindowsSpeechVoiceCommandId = command.Id;
                spokenForm.SpokenFormText = $"Sequel {item.Value}";
                _context.SaveChanges();
                CustomWindowsSpeechCommand action = new();
                action.WindowsSpeechVoiceCommandId = command.Id;
                action.TextToEnter = $" {item.Value} ";
                _context.CustomWindowsSpeechCommands.Add(action);
                _context.SaveChanges();
            }
        }
        public void CreateCommandKeywordCSharp()
        {
            var grammarName = _context.GrammarNames.Where(v => v.NameOfGrammar == "Keyword").FirstOrDefault();
            var keywordItems = _context.GrammarItems.Where(v => grammarName != null && v.GrammarNameId == grammarName.Id).ToList();
            foreach (GrammarItem item in keywordItems)
            {
                WindowsSpeechVoiceCommand command = new();
                command.ApplicationName = "Global";
                command.Description = $"Auto created: {DateTime.Now}";
                _context.WindowsSpeechVoiceCommands.Add(command);
                _context.SaveChanges();
                SpokenForm speakerForm = new SpokenForm();
                speakerForm.WindowsSpeechVoiceCommandId = command.Id;
                speakerForm.SpokenFormText = $"Keyword {item.Value}";
                _context.SaveChanges();
                CustomWindowsSpeechCommand action = new();
                action.WindowsSpeechVoiceCommandId = command.Id;
                action.TextToEnter = $" {item.Value} ";
                _context.CustomWindowsSpeechCommands.Add(action);
                _context.SaveChanges();
            }
        }
        public void CreateCommandMoveMouseAndClick()
        {
            var grammarName = _context.GrammarNames.Where(v => v.NameOfGrammar == "Direction").FirstOrDefault();
            var directions = _context.GrammarItems.Where(v => grammarName != null && v.GrammarNameId == grammarName.Id).ToList();
            grammarName = _context.GrammarNames.FirstOrDefault(f => f.NameOfGrammar == "1to30");
            var numbers = _context.GrammarItems.Where(v => grammarName != null && v.GrammarNameId == grammarName.Id).ToList();
            foreach (GrammarItem item in directions)
            {
                foreach (GrammarItem number in numbers)
                {
                    WindowsSpeechVoiceCommand command = new();
                    command.ApplicationName = "Global";
                    command.Description = $"Auto created: {DateTime.Now}";
                    _context.WindowsSpeechVoiceCommands.Add(command);
                    _context.SaveChanges();
                    SpokenForm spokenForm = new SpokenForm();
                    spokenForm.WindowsSpeechVoiceCommandId = command.Id;
                    spokenForm.SpokenFormText = $"{item.Value} {number.Value} Click";
                    _context.SaveChanges();
                    CustomWindowsSpeechCommand action = new();
                    action.WindowsSpeechVoiceCommandId = command.Id;
                    action.MouseCommand = "MoveMouseBy";
                    if (item.Value == "Left")
                    {
                        action.MouseMoveX = 0 - int.Parse(number.Value);
                    }
                    else if (item.Value == "Right")
                    {
                        action.MouseMoveX = 0 + int.Parse(number.Value);
                    }
                    else if (item.Value == "Up")
                    {
                        action.MouseMoveY = 0 - int.Parse(number.Value);
                    }
                    else if (item.Value == "Down")
                    {
                        action.MouseMoveY = 0 + int.Parse(number.Value);
                    }
                    _context.CustomWindowsSpeechCommands.Add(action);
                    CustomWindowsSpeechCommand action2 = new();
                    action2.WindowsSpeechVoiceCommandId = command.Id;
                    action2.MouseCommand = "LeftButtonDown";
                    _context.CustomWindowsSpeechCommands.Add(action2);
                    _context.SaveChanges();
                }
            }
        }
    }
}
