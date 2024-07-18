﻿using Microsoft.AspNetCore.Mvc;

namespace ProyectoIdentity.Controllers
{
    public class Heroe4Controller : Controller
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

                return RedirectToAction("Index", "Heroe4");
            }
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }
    }
}
