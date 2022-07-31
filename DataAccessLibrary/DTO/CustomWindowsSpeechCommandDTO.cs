
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
        public VirtualKeyCode? ModifierKey { get; set; } = VirtualKeyCode.NONAME;
        public VirtualKeyCode? KeyPressValue { get; set; } = VirtualKeyCode.NONAME;
        [StringLength(100)]
        public string MouseCommand { get; set; }
        [StringLength(100)]
        public string ProcessStart { get; set; }
        [StringLength(100)]
        public string CommandLineArguments { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        [Required]
        [StringLength(100)]
        public string SpokenCommand { get; set; } = "";
    }
}