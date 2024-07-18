﻿using Microsoft.AspNetCore.Mvc;

namespace ProyectoIdentity.Controllers
{
    public class Nieve9Controller : Controller
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
        public IActionResult Index(string accion)
        {
            if (accion == "Página siguiente")
            {

                return RedirectToAction("Create", "Nieve10");
            }
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }
    }
}
