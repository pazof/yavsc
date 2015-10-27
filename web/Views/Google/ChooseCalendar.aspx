<%@ Page Title="Google calendar usage" Language="C#" Inherits="System.Web.Mvc.ViewPage<CalendarList>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<% using ( Html.BeginForm("SetCalendar","Google") ) { %>
<input type="radio" name="calchoice" id ="c0" value="0" > 
<label for="c0">Calendrier intégré</label><br>
<% foreach (CalendarListEntry e in Model.items.Where(x=>x.accessRole=="owner")) { %>
<input type="radio" name="calchoice" id="c<%=e.id%>" value="<%=e.id%>" />
<label for="c<%=e.id%>"><%=Html.Encode(e.summary)%> <br>
<i><%=Html.Encode(e.description)%></i></label> <br>
<% } %>
<input type="hidden" name="returnUrl" id="returnUrl" value="<%=Html.Encode(ViewData["returnUrl"])%>">
<input type="submit">
<% } %>

</asp:Content>
