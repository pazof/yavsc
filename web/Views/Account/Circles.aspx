<%@ Page Title="Circles" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<script type="text/javascript" src="<%=Url.Content("~/Scripts/stupidtable.js")%>"></script> 
</asp:Content>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<table  id="tbc">
<thead>
<tr>
<th data-sort="string"><%=Html.Translate("Title")%></th>
</tr>
</thead>
<tbody id="tbcb">
<% int lc=0;
   foreach (var ci in (IEnumerable<Circle>) ViewData["Circles"]) { lc++; %>
<tr class="<%= (lc%2==0)?"even ":"odd " %>row" id="c_<%=ci.Id%>">
<td><%=Html.FormatCircle(ci)%></td>
   <td>
   <input type="button" value="<%=Html.Translate("Remove")%>" class="btnremovecircle actionlink" cid="<%=ci.Id%>"/>
    </td>
</tr>
<% } %>
</tbody>
</table>
<script>
$(function(){
$("#tbc").stupidtable();
});
</script>
</asp:Content>
<asp:Content ID="MASContentContent" ContentPlaceHolderID="MASContent" runat="server">

    <div id="dfnuser" class="hidden panel">
<%= Html.Partial("~/Views/Account/Register.ascx",new RegisterClientModel(),new ViewDataDictionary(ViewData)
    {
        TemplateInfo = new System.Web.Mvc.TemplateInfo
        {
            HtmlFieldPrefix = ViewData.TemplateInfo.HtmlFieldPrefix==""?"ur":ViewData.TemplateInfo.HtmlFieldPrefix+"_ur"
        }
    }) %>
 	</div>
<form>
<fieldset>
<legend>Nouveau cercle</legend>
<span id="msg" class="field-validation-valid error"></span>
<label for="title"><b><%=Html.Translate("Title")%></b></label>
<input type="text" id="title" name="title" class="inputtext"/>
<span id="Err_cr_title" class="field-validation-valid error"></span>
<table  id="tbmbrs">
<thead>
<tr>
<th data-sort="string"><%=Html.Translate("Members")%>
<span id="Err_cr_users" class="field-validation-valid error"></span>
	 <yavsc:InputUserName
	  id="users"
	  name="users"
	   emptyvalue="[nouvel utilisateur]"
	 onchange="onmembersChange(this.value);"
	 multiple="true"
	   runat="server" >
	 </yavsc:InputUserName> 
 	<script>
 	function message(msg) { 
  if (msg) { 
  $("#msg").removeClass("hidden");
  $("#msg").text(msg);
  } else { $("#msg").addClass("hidden"); } }

 	function onmembersChange(newval)
 	{
 		if (newval=='')
 			$("#dfnuser").removeClass("hidden");
 		else
 			$("#dfnuser").addClass("hidden");
 	}
 	function clearRegistrationValidation(){
    		$("#Err_ur_Name").text("");
            $("#Err_ur_UserName").text("");
            $("#Err_ur_Mobile").text("");
            $("#Err_ur_Phone").text("");
            $("#Err_ur_Email").text("");
            $("#Err_ur_Address").text("");
            $("#Err_ur_ZipCode").text("");
            $("#Err_ur_CityAndState").text("");
            $("#Err_ur_IsApprouved").text("");
    }
    function clearCircleValidation() {}
    function removeCircle() {
    	message(false);
    	 var id = $(this).attr('cid'); 
    	 $.ajax({
            url: "<%=Url.Content("~/api/Circle/Delete/")%>"+id,
            type: "GET",
            success: function (data) { 
            // Drops the detroyed circle row
            $("c_"+id).remove();
           },
            statusCode: {
            	400: function(data) {
            		$.each(data.responseJSON, function (key, value) {
            		var errspanid = "Err_cr_" + value.key.replace("model.","");
            		var errspan = document.getElementById(errspanid);
            		if (errspan==null)
            			alert('enoent '+errspanid);
            		else 
            			errspan.innerHTML=value.errors.join("<br/>");
                	});
            		}
            	},
            error: function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			message(xhr.status+" : "+xhr.responseText);
			    else message(false);
        		}});
    }

    function addCircle()
    {
    message(false);
  	  var circle = { title: $("#title").val(), users: $("#users").val() } ;
    	 $("#title").text('');
    	 $("#users").val('');
    	 $.ajax({
            url: "<%=Url.Content("~/api/Circle/Create")%>",
            type: "POST",
            data: circle,
            success: function (data) { 
            // Adds a node rendering the new circle
            $("#tbcb").append("<tr><td>"+circle.title+" <br><i>"+circle.users+"</i></td></tr>");
           },
            statusCode: {
            	400: function(data) {
            		$.each(data.responseJSON, function (key, value) {
            		var errspanid = "Err_cr_" + value.key.replace("model.","");
            		var errspan = document.getElementById(errspanid);
            		if (errspan==null)
            			alert('enoent '+errspanid);
            		else 
            			errspan.innerHTML=value.errors.join("<br/>");
                	});
            		}
            	},
            error: function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			message(xhr.status+" : "+xhr.responseText);
			    else message(false);
        		}});
    }

    function addUser()
    {
    message(false);
    	var user={
     	UserName: $("#ur_UserName").val(),
     	Name: $("#ur_Name").val(),
     	Password: $("#ur_Password").val(),
     	Email: $("#ur_Email").val(),
     	Address: $("#ur_Address").val(),
     	CityAndState: $("#ur_CityAndState").val(),
     	ZipCode: $("#ur_ZipCode").val(),
     	Phone: $("#ur_Phone").val(),
     	Mobile: $("#ur_Mobile").val(),
     	IsApprouved: true
     	};
     	clearRegistrationValidation();
        $.ajax({
            url: "<%=Url.Content("~/api/FrontOffice/Register")%>",
            type: "POST",
            data: user,
            success: function (data) { 
            	$("#users option:last").after($('<option>'+user.UserName+'</option>'));
           },
            statusCode: {
            	400: function(data) {
            		$.each(data.responseJSON, function (key, value) {
            		var errspanid = "Err_ur_" + value.key.replace("model.","");
            		var errspan = document.getElementById(errspanid);
            		if (errspan==null)
            			alert('enoent '+errspanid);
            		else 
            			errspan.innerHTML=value.errors.join("<br/>");
                	});
            		}
            	},
            error: function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			message(xhr.status+" : "+xhr.responseText);
			    else message(false);
        		}});
    }
    </script>

</th>
</tr>
</thead>
<tbody id="tbmbrsb">
</tbody>
</table>
<input type="button" id="btnnewcircle" value="<%=Html.Translate("Create")%>" class="actionlink rowbtnct" />
</fieldset>
</form>
<script>
 $(document).ready(function () {
 $("#btnnewuser").click(addUser);
 $("#btnnewcircle").click(addCircle);
 $(".btnremovecircle").click(removeCircle);
    });
 	</script>
</asp:Content>
