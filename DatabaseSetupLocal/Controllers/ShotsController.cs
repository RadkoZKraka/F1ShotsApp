﻿using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using DatabaseSetupLocal.Data;
using Microsoft.AspNetCore.Mvc;
using DatabaseSetupLocal.Models;
using DatabaseSetupLocal.Rep;
using DatabaseSetupLocal.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Extensions;


namespace DatabaseSetupLocal.Controllers;

[AllowAnonymous]
public class ShotsController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public ShotsRepository ShotsRepository { get; set; }
    public ShotsContext ShotsContext { get; set; }
    public UserRepository UserRepository { get; set; }


    public ShotsController(ILogger<HomeController> logger)
    {
        _logger = logger;

        this.ShotsRepository = new ShotsRepository(new ShotsContext());
        this.UserRepository = new UserRepository(new UsersContext());
        this.ShotsContext = ShotsRepository.GetShotsContext();
    }

    public IActionResult Index()
    {
        var users = ShotsRepository.GetUsers();
        var userId = User.Identity.GetUserId();
        ViewBag.AppUserId = userId;
        ViewBag.IsUserAdmin = UserRepository.GetIfUserIsAdminById(userId);

        return View(users.ToList());
    }

    public IActionResult Results()
    {
        var users = ShotsRepository.GetUsers().ToArray();
        var userId = User.Identity.GetUserId();
        ViewBag.AppUserId = userId;
        ViewBag.IsUserAdmin = UserRepository.GetIfUserIsAdminById(userId);
        var usersPoints = new List<(UserShots, List<int>)>();
        foreach (var user in users)
        {
            usersPoints.Add((user, ShotsRepository.GetUserPointsByYear(user.Id, DateTime.Now.Year)));
        }

        ViewBag.UsersPoints = usersPoints;
        return View(usersPoints);
    }

    public IActionResult Schedule()
    {
        var schedule = AppSetup.DeserializeDates();
        return View(schedule);
    }

    public ActionResult Races(string userId, int year)
    {
        ViewBag.User = ShotsRepository.GetUserById(userId);
        ViewBag.Year = year;
        var races = ShotsRepository.GetUserRacesById(userId).Where(x => x.RaceYear == year).ToList();
        if (races == null)
        {
            return HttpNotFound();
        }

        return View(races);
    }

    public ActionResult Shots(string userId, int raceId, string raceLocation)
    {
        ViewBag.User = ShotsRepository.GetUserById(userId);
        ViewBag.UserId = userId;
        ViewBag.Location = raceLocation;
        ViewBag.RaceId = raceId;
        ViewBag.Year = ShotsRepository.GetRaceYearById(raceId);

        var userIdentityId = User.Identity.GetUserId();
        ViewBag.HasAccessToEdit = ShotsRepository.GetUserById(userId).OwnerId == userIdentityId;
        ViewBag.IsAdmin = UserRepository.GetIfUserIsAdminById(userIdentityId);

        var race = ShotsRepository.GetRaceById(raceId);

        return View(race);
    }

    public ActionResult Years(string userId)
    {
        ViewBag.User = ShotsRepository.GetUserById(userId);
        ViewBag.UsersList = UserRepository.GetUsers();
        var years = ShotsRepository.GetUsersYears(userId);

        return View(years);
    }

    public ActionResult HideUser(string userId)
    {
        ShotsRepository.HideUser(userId);

        return RedirectToAction("Index");
    }

    public ActionResult ShowUser(string userId)
    {
        ShotsRepository.ShowUser(userId);

        return RedirectToAction("Index");
    }

    public ActionResult DeleteUser(string userId)
    {
        ShotsRepository.DeleteUser(userId);

        return RedirectToAction("Index");
    }

    public ActionResult GetRaceResults(int raceId)
    {
        ViewBag.UsersList = UserRepository.GetUsers();
        var race = ShotsRepository.GetRaceById(raceId);
        var results = F1WebScraper.GetRaceResults(race.RaceYear, race.RaceNo);
        var usersShots = race.Shot.Select(x => x.UsersShotDriver).ToList();
        var fullNameResult = AppSetup.AbrToFullName(results);
        var listOfPoints = ShotsRepository.CalculateUsersPoints(usersShots, fullNameResult);
        for (int i = 0; i < listOfPoints.Count; i++)
        {
            race.Shot[i].Points = listOfPoints[i];
            race.Shot[i].ResultDriver = results[i];
        }

        ShotsRepository.UpdateRace(race);
        return Redirect(HttpContext.Request.Headers["Referer"]);
    }
    

    public ActionResult EditOneShot(int? shotId)
    {
        if (shotId == null)
        {
            return NotFound();
        }

        var shot = ShotsContext.ShotModel.FirstOrDefault(s => s.Id == shotId);

        if (shot == null)
        {
            return HttpNotFound();
        }

        return View(shot);
    }

    [HttpPost, ActionName("EditOneShot")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOneShotPost(int? shotId)
    {
        if (shotId == null)
        {
            return NotFound();
        }

        var shotToUpdate = await ShotsContext.ShotModel.FirstOrDefaultAsync(s => s.Id == shotId);
        if (await TryUpdateModelAsync<Shot>(
                shotToUpdate,
                "",
                s => s.UsersShotDriver))
        {
            try
            {
                await ShotsContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists, " +
                                             "see your system administrator.");
            }
        }

        return View(shotToUpdate);
    }

    public ActionResult EditMultipleShots(int raceId, string userId)
    {
        ViewBag.User = ShotsRepository.GetUserById(userId);
        ViewBag.UserId = userId;
        ViewBag.Location = ShotsRepository.GetRaceById(raceId).RaceLocation;
        ViewBag.RaceId = raceId;
        ViewBag.Year = ShotsRepository.GetRaceById(raceId).RaceYear;
        ViewBag.PreviousUrl = HttpContext.Request.GetEncodedUrl();
        ViewBag.CurrentRace = AppSetup.GetCurrentRaceSchedule();


        var selectListItems = new List<string>();
        selectListItems.AddRange(AppSetup.DeserializeDrivers().Drivers.Select(x => x.FullName).ToList());
        ViewBag.F1Grid = selectListItems;

        if (raceId == null)
        {
            return NotFound();
        }

        var race = ShotsContext.RaceModel.FirstOrDefault(s => s.Id == raceId);

        if (race == null)
        {
            return HttpNotFound();
        }

        return View(race);
    }

    [HttpPost, ActionName("EditMultipleShots")]
    [AllowAnonymous]
    // [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMultipleShotsPost(int? raceId)
    {
        if (raceId == null)
        {
            return NotFound();
        }

        var shotsToUpdate = await ShotsContext.RaceModel.FirstOrDefaultAsync(s => s.Id == raceId);
        if (await TryUpdateModelAsync<Race>(
                shotsToUpdate,
                "",
                s => s.Shot))
        {
            try
            {
                await ShotsContext.SaveChangesAsync();
                return Redirect(Request.GetEncodedUrl());
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists, " +
                                             "see your system administrator.");
            }
        }

        return View(shotsToUpdate);
    }

    public ActionResult AddUser(string userId)
    {
        if (String.IsNullOrEmpty(userId))
        {
            return HttpNotFound();
        }

        var user = UserRepository.GetUserById(userId);
        var shots = AppSetup.SetupShotsForNewUser(userId, user.FirstName + " " + user.LastName);
        ShotsRepository.InsertUser(shots);
        return RedirectToAction("Index");
    }

    public ActionResult AssignUser(string userId)
    {
        if (String.IsNullOrEmpty(userId))
        {
            return HttpNotFound();
        }

        var user = ShotsRepository.GetUserById(userId);
        ViewBag.UsersList = UserRepository.GetUsers().Select(x => new{x.Id, x.FirstName, x.LastName});
        return View(user);
    }

    [HttpPost, ActionName("AssignUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignUserPost(string userId)
    {

        var userModelToUpdate = await ShotsContext.UserModel.FirstOrDefaultAsync(s => s.Id == userId);
        if (await TryUpdateModelAsync<UserShots>(
                userModelToUpdate,
                "",
                s => s.OwnerId))
        {
            try
            {
                await ShotsContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists, " +
                                             "see your system administrator.");
            }
        }

        return View("../Home/Index");
    }


    public ActionResult CurrentRace()
    {
        var userIdentityId = User.Identity.GetUserId();
        var userShot = ShotsRepository.GetUserByOwnerId(userIdentityId);

        ViewBag.User = ShotsRepository.GetUserByOwnerId(userIdentityId);
        ViewBag.UserId = userShot.Id;
        ViewBag.RaceId = ShotsRepository.GetRaceIdByRaceLoc(userShot.Id, AppSetup.GetCurrentRaceLocation());
        ViewBag.Location = AppSetup.GetCurrentRaceLocation();

        var userId = ShotsRepository.GetUserIdByOwnerId(userIdentityId);
        ViewBag.HasAccessToEdit = ShotsRepository.GetUserById(userId).OwnerId == userIdentityId;
        var shots = ShotsRepository.GetUserShotsByUserIdAndRaceLoc(userId, AppSetup.GetCurrentRaceLocation());
        if (shots == null)
        {
            return HttpNotFound();
        }

        return View(shots);
    }

    public ActionResult LiveTiming()
    {
        var userIdentityId = User.Identity.GetUserId();
        var userShot = ShotsRepository.GetUserByOwnerId(userIdentityId);

        ViewBag.User = ShotsRepository.GetUserByOwnerId(userIdentityId);
        ViewBag.UserId = userShot.Id;
        ViewBag.RaceId = ShotsRepository.GetRaceIdByRaceLoc(userShot.Id, AppSetup.GetCurrentRaceLocation());
        ViewBag.Location = AppSetup.GetCurrentRaceLocation();

        var userId = ShotsRepository.GetUserIdByOwnerId(userIdentityId);
        ViewBag.HasAccessToEdit = ShotsRepository.GetUserById(userId).OwnerId == userIdentityId;
        var shots = ShotsRepository.GetUserShotsByUserIdAndRaceLoc(userId, AppSetup.GetCurrentRaceLocation());
        if (shots == null)
        {
            return HttpNotFound();
        }

        return View(shots);
    }

    [HttpGet]
    public JsonResult GetLiveTiming()
    {
        var res = F1WebScraper.GetLiveData();
        var model = new JsonResponseViewModel();
        if (res != null)
        {
            model.ResponseCode = 0;
            model.ResponseMessage = JsonConvert.SerializeObject(res);
        }
        else
        {
            model.ResponseCode = 1;
            model.ResponseMessage = "Error";
        }

        return Json(model);
    }

    private ActionResult HttpNotFound()
    {
        throw new NotImplementedException();
    }
}