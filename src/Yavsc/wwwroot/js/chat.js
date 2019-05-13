+(function ($) {
/*
# chat control ids 

## inputs 

* msg_<rname> : input providing a room message
* pv_<uname>  : input providing a private message
* command : input providing server command
* roomname : input (hidden or not) providing a room name

## persistent containers

* chatview : the global container
* rooms : container from room displays
* pvs : container from pv displays
* server : container for server messages

## temporary containers
  
* room_<rname> : room message list
* pv_<uname> : private discussion display

## css classes

* form-control
* btn btn-default

*/
  
  var ChatView = function ($view, full)
  {

    if (!full) throw "not implemented";
    var chat = $.connection.chatHub
    // Create a function that the hub can call back to display messages.
    chat.client.addMessage = function (name, room, message) {
      // Add the message to the page.
      $('#room_'+room).append('<li class="discussion"><strong>' + htmlEncode(name)
        + '</strong>: ' + htmlEncode(message) + '</li>')
    }

    chat.client.addPV = function (name, message) {
      if (!$('#mute').prop('checked')) {
        audio.play()
      }
      // Add the pv to the page.
      $('#pv_'+name).append('<li class="pv"><strong>' + htmlEncode(name)
        + '</strong>: ' + htmlEncode(message) + '</li>')
    }
    chat.client.notify = function (tag, message, data) {
      if (data) {
        // Add the notification to the page.
        if (tag === 'connected') {
          onUserConnected(ulist, message, data)
          $('#notifications').append('<li class="notif"><i>' + htmlEncode(tag)
            + '</i> ' + htmlEncode(data) + '</li>')
        } 
        else if (tag === 'disconnected') {
          onUserDisconnected(ulist, message, data)
          $('#notifications').append('<li class="notif"><i>' + htmlEncode(tag)
            + '</i> ' + htmlEncode(data) + '</li>')
        } // reconnected userpart userjoin deniedpv
        else {
          $('#notifications').append('<li class="notif"><i>' + htmlEncode(tag)
            + '</i> ' + htmlEncode(message) + ' : <code>' + htmlEncode(data) + '</code></li>')
        }
      }
    }
    
    chat.client.joint =  function (rinfo) 
    {
        console.log(rinfo);
        setActiveRoom(rinfo.Name);
    }

    $.fn.filterByData = function (prop, val) {
      return this.filter(
        function () { return $(this).data(prop) == val; }
      )
    }

    var activeRoom;
    var activeRoomName;
    var setActiveRoom = function(room) {
      if (activeRoom) {
        // TODO animate
        activeRoom.addClass("hidden");
        $("sel_"+activeRoomName).addClass("btn-primary");
      } 
      activeRoom=$("#vroom_"+room);
      activeRoomName=room;
      activeRoom.removeClass("hidden");
    }

    var roomlist = $('<div class="roomlist"></div>');
    roomlist.appendTo($view);
    var chatlist = $('<div class="chatlist" ></div>');
    chatlist.appendTo($view);
  
    var buildRoom = function (room)
    {
      var roomTag =  $("<a>"+room+"</a>").addClass("btn").addClass("btn-primary");
      roomTag.prop("id","sel_"+room)
      .click(function(){
        setActiveRoom(room);
        $(this).removeClass("btn-primary");
      });
      roomTag.appendTo(roomlist);
      var roomview = $("<div></div>").addClass("container");
      roomview.appendTo(chatlist);
      roomview.prop('id',"vroom_"+room);
      var msglist = $("<ul></ul>").addClass("mesglist");
      msglist.prop('id',"room_"+room);
      msglist.appendTo(roomview);
      $("<input type=\"text\">")
      .prop('id','inp_'+room)
      .prop('enable',false)
      .prop('hint','hello')
      .prop('title','send to '+room)
      .addClass('form-control')
      .keydown(function(ev) {
        if (ev.which == 13) {
          if (this.value.length==0) return;
          console.log("sending to "+room+" "+this.value)
          chat.server.send(room, this.value);
          this.value="";
        }}).appendTo(roomview);
      chans.push(room);
      setActiveRoom(room);
    }

    // build a channel list
    var chans = Array();

    $view.data("chans").split(",").forEach(function(chan) {
      buildRoom(chan)
    });
    

    function onCx () {
      setTimeout(function () {
        getUsers()
      }, 120),
      $('#chatview').removeClass('disabled');

      chans.forEach(function(room) {
        chat.server.join(room);
      })
    }

    function onDisCx () {
      $('#chatview').addClass('disabled');
    }

    // Start the connection.
    $.connection.hub.start().done(function () {
      onCx()
    })

    $.connection.hub.disconnected(function () {

      onDisCx()
      setTimeout(function () {
        $.connection.hub.start().done(function () {
          // $('#mySignalRConnectionIdHidden').val($.connection.hub.id)
          onCx()
        }, 30000); // Re-start connection after 30 seconds
      })
    })
    
    

    $("<label for=\"channame\">&gt;&nbsp;</label>")
    .appendTo($view);
    var chanName = $("<input name=\"channame\" title=\"channel name\" hint=\"yavsc\">");
    chanName.appendTo($view);
    
    chanName.keydown(
      function (event) {
      if (event.which == 13) {
        if (this.value.length==0) return;
        buildRoom(this.value);
        chat.server.join(this.value);
        this.value=""
      } else {
       // TODO showRoomInfo(this.value);
      }
    });

    var ulist = $("<ul></ul>").addClass('userlist');

    ulist.appendTo($view);

    
    var pvuis
    // TODO get this data from the chatview element
    var audio = new Audio('/sounds/bell.mp3')
    
    var sendPV = function () {
      var msg = $('#pv').val()
      // Call the Send method on the hub.
      $.each(pvuis.CXs, function () {
        chat.server.sendPV(this, msg)
      })
      $('#discussion').append('<li class="pv">' + htmlEncode(pvuis.UserName)
        + '<< ' + htmlEncode(msg) + '</li>')
      // Clear text box and reset focus for next comment.
      $('#pv').val('')
    }

    $('#pv').keydown(function (event) {
      if (event.which == 13) {
        sendPV()
      }
    })

    $('#command').keydown(function (event) {
      if (event.which == 13) {
        sendCommand()
      }
    })

    function addChatUser (ulist, uname, cxids) {
      $('<li class="user"><img src="/Avatars/' + uname + '.xs.png"> ' + uname + '</li>')
        .data('name', uname)
        .data('cxids', cxids)
        .css('cursor', 'pointer')
        .click(function () { setPrivateTarget(this); })
        .appendTo(ulist)
    }


    function getUsers (ulist) {
      $('#userlist').empty()
      $('#to').empty()
      $.get('/api/chat/users').done(
        function (users) {
          $.each(users, function () {
            var user = this
            var existent = $('#userlist li').filterByData('name', user.UserName)
            if (existent.length > 0) existent.remove()
            var cxids = []
            $.each(user.Connections, function () {
              cxids.push(this.ConnectionId)
            })
            addChatUser(ulist,user.UserName, cxids)
          })
        }
      )
    }

    
    // This optional function html-encodes messages for display in the page.
    function htmlEncode (value) {
      var encodedValue = $('<div />').text(value).html()
      return encodedValue
    }

    var setPrivateTarget = function (li) {
      $('#rooms').addClass('hidden')
      $('#sendpvbox').removeClass('hidden')
      pvuis = { CXs: $(li).data('cxids'), UserName: $(li).data('name') }
      $('#sendpvdest').html(pvuis.UserName)
      $('#pvs').focus()
    }
    var setPublic = function () {
      $('#rooms').removeClass('hidden')
      $('#sendpvbox').addClass('hidden')
      $('#message').focus()
    }
    $('#pubChan').css('cursor', 'pointer')
    $('#pubChan').click(setPublic)
    setPublic()
    // Reference the auto-generated proxy for the hub.
    

    var onUserDisconnected = function (cxid) {
      $('#userlist li').filter(function () {
        var nids = $(this).data('cxids').filter(function () {
          return $(this) !== cxid
        });
        if (nids.length==0) $(this).remove()
        else $(this).data('cxids', nids)
      })
    }

    var onUserConnected = function (ulist, cxid, username) {
      var connected = $('#userlist li').filterByData('name', username)
      if (connected.length > 0) {
        var ids = connected.data('cxids')
        ids.push(cxid)
        connected.data('cxids', ids)
      } else {
        addChatUser(ulist, username, [cxid])
      }
    }
    
    $(window).unload(function () { chat.server.abort(); })

  }

  $(document).ready(function($){
    ChatView($('#chatview'),true);
  });

})(jQuery);
