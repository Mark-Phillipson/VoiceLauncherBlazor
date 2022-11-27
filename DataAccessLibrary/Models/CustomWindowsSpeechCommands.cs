using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WindowsInput.Native;

namespace DataAccessLibrary.Models
{
    public class CustomWindowsSpeechCommand
    {
        public int Id { get; set; }
        public int WindowsSpeechVoiceCommandId { get; set; }
        public virtual WindowsSpeechVoiceCommand WindowsSpeechVoiceCommand { get; set; }
        [StringLength(255)]
        public string TextToEnter { get; set; } = "";
        [StringLength(40)]
        public string SendKeysValue { get; set; }
        public VirtualKeyCode? KeyDownValue { get; set; } = VirtualKeyCode.NONAME;
        //To be depreciated
        public VirtualKeyCode? ModifierKey { get; set; } = VirtualKeyCode.NONAME;
        public bool ControlKey { get; set; } = false;
        public bool ShiftKey { get; set; } = false;
        public bool AlternateKey { get; set; } = false;
        public bool WindowsKey { get; set; } = false;
        public VirtualKeyCode? KeyPressValue { get; set; } = VirtualKeyCode.NONAME;
        public VirtualKeyCode? KeyUpValue { get; set; } = VirtualKeyCode.NONAME;
        [StringLength(100)]
        public string MouseCommand { get; set; } = "";
        public int MouseMoveX { get; set; } = 0;
        public int MouseMoveY { get; set; } = 0;
        public double AbsoluteX { get; set; } = 0;
        public double AbsoluteY { get; set; } = 0;
        public int ScrollAmount { get; set; } = 0;
        [StringLength(255)]
        public string ProcessStart { get; set; } = "";
        [StringLength(255)]
        public string CommandLineArguments { get; set; } = "";
        public int WaitTime { get; set; }
        [StringLength(55)]
        public string HowToFormatDictation { get; set; }="Do Nothing";
        [StringLength(255)]
        public string MethodToCall { get; set; }

    }
}
