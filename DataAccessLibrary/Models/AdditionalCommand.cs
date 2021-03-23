using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
	public partial class AdditionalCommand
	{
		[Key]
		[Column("ID")]
		public int Id { get; set; }
		[Column("CustomIntelliSenseID")]
		public int CustomIntelliSenseId { get; set; }
		[Required]
		[Column("WaitBefore")]
		public decimal WaitBefore { get; set; } = (decimal)0.1;
		[Required(AllowEmptyStrings = false)]
		[Column("SendKeys_Value")]
		[Display(Name ="Send Keys Value")]
		public string SendKeysValue { get; set; }
		[StringLength(255)]
		public string Remarks { get; set; }
		[Required(AllowEmptyStrings = false)]
		[StringLength(30)]
		[Display(Name ="Delivery Type")]
		public string DeliveryType { get; set; }

		[ForeignKey(nameof(CustomIntelliSenseId))]
		[InverseProperty(nameof(Models.CustomIntelliSense.AdditionalCommands))]
		public virtual CustomIntelliSense CustomIntelliSense { get; set; }
	}
}

