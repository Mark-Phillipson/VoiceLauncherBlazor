using System.ComponentModel.DataAnnotations;

using WindowsInput.Native;

namespace DataAccessLibrary.DTO
{
    public partial class CustomWindowsSpeechCommandDTO
    {
        [Key]
        public int Id { get; set; }
        [StringLength(100)]
        public string TextToEnter { get; set; }
        public VirtualKeyCode? KeyDownValue { get; set; } = VirtualKeyCode.NONAME;
        //[Depreciated]
        public VirtualKeyCode? ModifierKey { get; set; } = VirtualKeyCode.NONAME;
        public bool ControlKey { get; set; } = false;
        public bool ShiftKey { get; set; }=false;
        public bool AlternateKey { get; set; } = false;
        public bool WindowsKey { get; set; } = false;
        public VirtualKeyCode? KeyPressValue { get; set; } = VirtualKeyCode.NONAME;
        public VirtualKeyCode? KeyUpValue { get; set; } = VirtualKeyCode.NONAME;

        [StringLength(100)]
        public string MouseCommand { get; set; }
        public int MouseMoveX { get; set; } = 0;
        public int MouseMoveY { get; set; } = 0;
        public double AbsoluteX { get; set; } = 0;
        public double AbsoluteY { get; set; } = 0;
        public int ScrollAmount { get; set; } = 0;
        [StringLength(100)]
        public string ProcessStart { get; set; }
        [StringLength(100)]
        public string CommandLineArguments { get; set; }
        [Required]
        public int WindowsSpeechVoiceCommandId { get; set; }
        public int WaitTime { get; set; }= 100;
    }
}