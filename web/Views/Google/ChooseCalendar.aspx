<%@ Page Title="Google calendar usage" Language="C#" Inherits="System.Web.Mvc.ViewPage<CalendarList>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<% using ( Html.BeginForm("SetCalendar","Google") ) { %>
<% foreach (CalendarListEntry e in Model.items.Where(x=>x.accessRole=="owner")) { %>
<input type="radio" name="calchoice" id="calchoice" value="<%=e.id%>" >
<%=Html.Encode(e.summary)%> <br>
<i><%=Html.Encode(e.description)%></i> <br>
<% } %>
<input type="submit">
<% } %>

</asp:Content>
