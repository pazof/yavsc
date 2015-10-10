<%@ Page Title="Db init" Language="C#" MasterPageFile="~/Models/NoLogin.master" Inherits="System.Web.Mvc.ViewPage<TaskOutput>" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<h1>Initialisation de la base de données</h1>
<div><h2>Error message </h2> <%= Html.Encode(Model.Error) %></div>
<div><h2>Message </h2> <%= Html.Encode(Model.Message) %></div>
<div><h2>Exit Code</h2> <%= Html.Encode(Model.ExitCode) %></div>
<form><fieldset><legend>Acces à la base de donnée</legend>
<label>db Name:</label><%= Html.Encode(ViewData["DbName"]) %><br/>
<label>db User:</label><%= Html.Encode(ViewData["DbUser"]) %><br/>
<label>Hôte:</label><%= Html.Encode(ViewData["Host"]) %></fieldset>
</form>
</asp:Content>
