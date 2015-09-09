<%@ Page Title="Circles" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<script type="text/javascript" src="<%=Url.Content("~/Scripts/stupidtable.js")%>"></script> 
</asp:Content>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<table  id="tbc">
<thead>
<tr>
<th data-sort="string"><%=Html.Translate("Title")%>
</th>
</tr>
</thead>
<tbody id="tbcb">
<% int lc=0;
   foreach (var ci in (IEnumerable<CircleBase>) ViewData["Circles"]) { lc++; %>
<tr class="<%= (lc%2==0)?"even ":"odd " %>row" id="c_<%=ci.Id%>">
<td cid="<%=ci.Id%>" style="cursor: pointer;" class="btnselcircle" >
<%=ci.Title%></td>
   <td>
   <input type="button" value="<%=Html.Translate("Remove")%>" 
   class="btnremovecircle actionlink" cid="<%=ci.Id%>"/>
    </td>
</tr>
<% } %>
</tbody>
</table>

<div class="actionlink" id="btednvcirc" did="fncirc">Ajouter un cercle</div>
<script>
$(function(){
$("#tbc").stupidtable();
});
</script>
</asp:Content>
<asp:Content ID="MASContentContent" ContentPlaceHolderID="MASContent" runat="server">
<div id="fncirc" class="hidden">


<div class="panel">
<form>
<fieldset>
<legend id="lgdnvcirc"></legend>
<span id="msg" class="field-validation-valid error"></span>

<label for="title"><b><%=Html.Translate("Title")%></b></label>
<input type="text" id="title" name="title" class="inputtext" onchange="onCircleChanged"/>
<span id="Err_cr_title" class="field-validation-valid error"></span>

<label for="isprivate"><b><%=Html.Translate("Private circle")%></b></label>
<input type="checkbox" name="isprivate" id="isprivate" onchange="onCircleChanged" />

<input type="button" id="btnnewcircle" 
value="<%=Html.Translate("Create")%>" class="actionlink rowbtnct" />
<input type="button" id="btneditcircle" 
value="<%=Html.Translate("Modify")%>" class="actionlink rowbtnct" />
<input type="hidden" name="id" id="id" />

</fieldset>
</form>
</div>
</div>
<script type="text/javascript"  src="<%=Url.Content("~/Scripts/yavsc.circles.js")%>" >
</script>
<script type="text/javascript">

 $(document).ready(function () {
 $('#btednvcirc').click(editNewCircle);
 $("#btnnewcircle").click(addCircle);
 $("#btneditcircle").click(modifyCircle);
 $(".btnremovecircle").click(removeCircle);
 $(".btnselcircle").click(selectCircle);
    });

 	</script>
</asp:Content>
