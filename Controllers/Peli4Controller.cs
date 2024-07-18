﻿using Microsoft.AspNetCore.Mvc;

namespace ProyectoIdentity.Controllers
{
    public class Peli4Controller : Controller
    {
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string accion)
        {
            if (accion == "Página siguiente")
            {

                return RedirectToAction("Index", "Peli4");
            }
            return View();
        }
    }
}
