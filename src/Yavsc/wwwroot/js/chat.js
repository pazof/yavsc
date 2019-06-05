window.ChatHubHandler = (function ($) {

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
  $.fn.filterByData = function (prop, val) {
    return this.filter(function () { return $(this).data(prop) == val; });
  };
  var ChatView = function ($view, full) {

    if (!full) throw new Error('not implemented');

    // build a channel list
    var chans = [];
    var frontRoomName;

    var ulist = $('<ul></ul>').addClass('userlist');
    var notifications = $('<ul></ul>').addClass('notifs');

    ulist.appendTo($view);
    notifications.appendTo($view);

    var chat = $.connection.chatHub;
    // Create a function that the hub can call back to display messages.
    chat.client.addMessage = function (name, room, message) {
      // Add the message to the page.
      $('#room_' + room).append('<li class="discussion"><strong>' + htmlEncode(name) + '</strong>: ' + htmlEncode(message) + '</li>');
    };

    chat.client.addPV = function (name, message) {
      if (!$('#mute').prop('checked')) {
        audio.play();
      }
      // Add the pv to the page.
      $('#pv_' + name).append('<li class="pv"><strong>' + htmlEncode(name) + '</strong>: ' + htmlEncode(message) + '</li>');
    };

    chat.client.notifyRoom = function (tag, targetid, message) {
      // Add the notification to the page.
      if (tag === 'connected' || tag === 'reconnected') onUserConnected(targetid, message);
      else if (tag === 'disconnected') onUserDisconnected(targetid, message);
      // eslint-disable-next-line no-warning-comments
      // TODO reconnected userpart userjoin deniedpv
      $('<li></li>').addClass(tag).append(tag + ': ' + targetid + ' ').append(message).addClass(tag).appendTo($('#room_' + targetid));
    };

    chat.client.notifyUser = function (tag, targetid, message) {
      // Add the notification to the page.
      if (tag === 'connected' || tag === 'reconnected') onUserConnected(targetid, message);
      else if (tag === 'disconnected') onUserDisconnected(targetid, message);
      // eslint-disable-next-line no-warning-comments
      // TODO reconnected userpart userjoin deniedpv
      $('<li></li>').append(tag + ': ' + targetid).append(message).addClass(tag).appendTo(notifications);
    };

    var setActiveRoom = function (room) {
      var frontRoom;
      if (frontRoomName !== '') {
        frontRoom = $('#vroom_' + frontRoomName);
        // eslint-disable-next-line no-warning-comments
        // TODO animate
        frontRoom.addClass('hidden');
        $('#sel_' + frontRoomName).addClass('btn-primary');
      }
      frontRoomName = room;
      frontRoom = $('#vroom_' + room);
      $('#sel_' + room).removeClass('btn-primary');
      frontRoom.removeClass('hidden');
    };

    var chatbar = $('<div class="chatbar"></div>');

    var roomjoin = $('<div class="chatctl" class="form-control"></div>');

    var roomlist = $('<div class="roomlist"></div>');
    roomlist.appendTo(chatbar);
    $('<label for="channame">Join&nbsp;:</label>')
      .appendTo(roomjoin);
    var chanName = $('<input id="channame" title="channel name" hint="yavsc"  >');
    chanName.appendTo(roomjoin);
    roomjoin.appendTo(chatbar);

    chatbar.appendTo($view);
    var chatlist = $('<div class="chatlist" ></div>');
    chatlist.appendTo($view);
    var buildRoom = function (room) {
      var roomTag = $('<a>' + room + '</a>').addClass('btn');
      roomTag.prop('id', 'sel_' + room).click(function () {
        setActiveRoom(room);
        $(this).removeClass('btn-primary');
      });

      roomTag.appendTo(roomlist);
      var roomview = $('<div></div>').addClass('container');
      roomview.appendTo(chatlist);
      roomview.prop('id', 'vroom_' + room);
      var msglist = $('<ul></ul>').addClass('mesglist');
      msglist.prop('id', 'room_' + room);
      msglist.appendTo(roomview);
      $('<input type="text">')
        .prop('id', 'inp_' + room)
        .prop('enable', false)
        .prop('hint', 'hello')
        .prop('title', 'send to ' + room)
        .addClass('form-control')
        .keydown(function (ev) {
          if (ev.which == 13) {
            if (this.value.length == 0) return;
            chat.server.send(room, this.value);
            this.value = '';
          }
        }).appendTo(roomview);
      chans.push(room);
      setActiveRoom(room);
    };

    $view.data('chans').split(',').forEach(function (chan) {
      buildRoom(chan);
    });

    function onCx() {
      setTimeout(function () {
        getUsers();
      }, 120);

      $('#chatview').removeClass('disabled');

      chans.forEach(function (room) {
        chat.server.join(room).done(function (chatInfo) {
          setActiveRoom(chatInfo.Name);
        });
      });
    }

    function onDisCx() {
      $('#chatview').addClass('disabled');
    }

    // Start the connection.
    $.connection.hub.start().done(function () {
      onCx();
    });

    $.connection.hub.disconnected(function () {

      onDisCx();
      setTimeout(function () {
        $.connection.hub.start().done(function () {
          // $('#mySignalRConnectionIdHidden').val($.connection.hub.id)
          onCx();
        }, 30000); // Re-start connection after 30 seconds
      });
    });

    chanName.keydown(function (event) {
      if (event.which == 13) {
        if (this.value.length == 0) return;
        buildRoom(this.value);
        chat.server.join(this.value).done(function (chatInfo) {
          setActiveRoom(chatInfo.Name);
        });
        this.value = '';
      }
      // else TODO showRoomInfo(this.value);
    });

    var pvuis;
    // eslint-disable-next-line no-warning-comments
    // TODO get this data from the chatview element
    var audio = new Audio('/sounds/bell.mp3');

    var sendPV = function () {
      var msg = $('#pv').val();
      // Call the Send method on the hub.
      $.each(pvuis.CXs, function () {
        chat.server.sendPV(this, msg);
      });
      $('#discussion').append('<li class="pv">' + htmlEncode(pvuis.UserName) + '<< ' + htmlEncode(msg) + '</li>');
      // Clear text box and reset focus for next comment.
      $('#pv').val('');
    };

    $('#pv').keydown(function (event) {
      if (event.which == 13) {
        sendPV();
      }
    });

    $('#command').keydown(function (event) {
      if (event.which == 13) {
        // eslint-disable-next-line no-warning-comments
        // TODO server command: sendCommand()
      }
    });

    var addChatUser = function (uname) {

      $('#u_' + uname).remove();
      // ulist.remove("li.user[data='"+uname+"']");

      $('<li class="user"><img src="/Avatars/' + uname + '.xs.png"> ' + uname + '</li>')
        .prop('id', 'u_' + uname)
        .css('cursor', 'pointer')
        .click(function () { setPrivateTarget(this); })
        .appendTo(ulist);
    };

    var getUsers = function () {
      $.get('/api/chat/users').done(function (users) {
        $.each(users, function () {
          var user = this;
          addChatUser(user.UserName);
        });
      });
    };

    // This optional function html-encodes messages for display in the page.
    function htmlEncode(value) {
      var encodedValue = $('<div />').text(value).html();
      return encodedValue;
    }

    var setPrivateTarget = function (li) {
      $('#rooms').addClass('hidden');
      $('#sendpvbox').removeClass('hidden');
      pvuis = { CXs: $(li).data('cxids'), UserName: $(li).data('name') };
      $('#sendpvdest').html(pvuis.UserName);
      $('#pvs').focus();
    };

    var setPublic = function () {
      $('#rooms').removeClass('hidden');
      $('#sendpvbox').addClass('hidden');
      $('#message').focus();
    };

    $('#pubChan').css('cursor', 'pointer');
    $('#pubChan').click(setPublic);
    setPublic();

    var onUserDisconnected = function (uname) {
      $('#u_' + uname).remove();
    };

    var onUserConnected = function (username) {
      addChatUser(username);
    };

    $(window).unload(function () { chat.server.abort(); });

  };

  $(document).ready(function ($) {
    ChatView($('#chatview'), true);
  });

})(window.jQuery);
