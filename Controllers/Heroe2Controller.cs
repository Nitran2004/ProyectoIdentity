﻿using Microsoft.AspNetCore.Mvc;

namespace ProyectoIdentity.Controllers
{
    public class Heroe2Controller : Controller
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

                return RedirectToAction("Index", "Heroe2");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string accion)
        {
            if (accion == "Página siguiente")
            {

                return RedirectToAction("Create", "Heroe3");
            }
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
