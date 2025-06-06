﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class LauncherMultipleLauncherBridge
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column("LauncherID")]
        public int LauncherId { get; set; }
        public Launcher Launcher { get; set; } = null!;
        [Column("MultipleLauncherID")]
        public int MultipleLauncherId { get; set; }
        public MultipleLauncher MultipleLauncher { get; set; } = null!;
    }
}
