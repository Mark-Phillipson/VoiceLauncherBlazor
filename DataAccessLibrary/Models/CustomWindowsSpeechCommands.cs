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
        [StringLength( 100 )]
        [Required]
        public string SpokenCommand { get; set; }
        [StringLength(100)]
        public string TextToEnter { get; set; } = "";
        public VirtualKeyCode? KeyDownValue { get; set; }
        public VirtualKeyCode? ModifierKey { get; set; }
        public VirtualKeyCode? KeyPressValue { get; set; }
        [StringLength(100)]
        public string MouseCommand { get; set; }="";
        [StringLength(100)]
        public string ProcessStart { get; set; } = "";
        [StringLength(100)]
        public string CommandLineArguments { get; set; } = "";
        [StringLength(1000)]
        public string Description { get; set; }

    }
}
