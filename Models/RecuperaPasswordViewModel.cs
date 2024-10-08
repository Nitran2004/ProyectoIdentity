﻿using System.ComponentModel.DataAnnotations;

namespace ProyectoIdentity.Models
{
    public class RecuperaPasswordViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email es inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación de la contraseña no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }



    }
}
