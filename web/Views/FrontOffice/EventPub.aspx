<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<Yavsc.ApiControllers.Calendar.Model.EventPub>" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<% using (Html.BeginForm()) { %>

<%= Html.LabelFor(model => model.Title) %>: <%=Model.Title%><br/>
<%= Html.LabelFor(model => model.Description) %>: <%=Model.Description%> <br/>
<%= Html.LabelFor(model => model.Location) %>: <%=Model.Location%> <br/>
<%= Html.LabelFor(model => model.StartDate) %>: <%=Model.StartDate%> <br/>
<%= Html.LabelFor(model => model.EndDate) %>: <%=Model.EndDate%> <br/>
<%= Html.LabelFor(model => model.Circles) %>: <%=Model.Circles%> <br/>
<%= Html.LabelFor(model => model.ImgLocator) %>: <%=Model.ImgLocator%> <br/>
<%= Html.LabelFor(model => model.EventWebPage) %>: <%=Model.EventWebPage%> <br/>
<%= Html.LabelFor(model => model.ProviderName) %>: <%=Model.ProviderName%> <br/>
<%= Html.LabelFor(model => model.Comment) %>: <%=Model.Comment%> <br/>

<% } %>




</asp:Content>