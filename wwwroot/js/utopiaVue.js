
Vue.component('button-counter', {
    data: function () {
        return {
            count: 0
        }
    },
    template: '<button v-on:click="count++" >You clicked me {{ count }} times.</button>'
})

Vue.component("progress-bar", 
    {
        data: function () 
        {
            return {
                width: 0
            }
        },
        computed:
            {
                
            },
        
        template: '<div id="myProgress"> '+
                    '<div id="myBar"></div>' +
                    ' </div>',
        
        methods:
            {
            move (i) 
            {
                if (i === 0) 
                {
                    i = 1;
                    let width = 1;
                    let id = setInterval(frame, 10);
                    function frame() {
                        if (width >= 1000) {
                            i = 0;
                        } else {
                            width++;
                            width = width/10 + "%";
                        }
                    }
                }
                
            },
            initMove()
            {
                console.log("Moving!")
                setInterval(self.move(0), 10000)
            },
            stopMove() {
                console.log("Stopping move!")
                this.width = 0;
                clearInterval(self.move);
            },
            mounted() {
                this.$root.$on('startProgress', () => {
                    this.initMove();
                })
            }

            }
    });

Vue.component('button-move', {
    data: function () {
        return {
            gathering: false,
            time: 0,
            timer: null,
            timerOn: false,
            hexX: 0,
            hexY: 0,
        }
    },
    props: {
        action: Object,


    },
    methods:{
        movePlayer: function () {
            console.log("moving player!");

            $.ajax({
                url: "/Home/MovePlayer",

                data: {hexX: this.hexX, hexY: this.hexY },
                type: "POST",
                success: function(data)
                {
                    console.log(data);
                },
                error: function(passParams)
                {
                    console.log(passParams);
                }
            });
        }
    },

    template: '<button class="btn btn-primary"   ' +
        'v-on:click="movePlayer(), $root.newLog(moving player), $root.gatherResource(action.resource), $root.initMove() ">' +
        '<span >Move player</span>' +
        '</button>'
})

Vue.component('button-action', {
    data: function () {
        return {
            gathering: false,
            time: 0,
            timer: null,
            timerOn: false
        }
    },
    props: {
        action: Object,


    },
    methods: {
        countdown() {
            this.timerOn = true;
            console.log("Timer: " + this.timer)
            console.log("Time: " + this.time)

            if (!this.timer) {
                this.timer = setInterval(() => {
                    if (this.time > 0) {
                        this.time--
                        console.log("Timer: " + this.timer)
                        console.log("Time: " + this.time)
                    } else {
                        clearInterval(this.timer)
                        this.reset()
                        console.log("Timer: " + this.timer)
                        console.log("Time: " + this.time)
                    }
                }, 1000)
            }
        },
        stop() {
            this.timerOn = false
            clearInterval(this.timer)
            this.timer = null

        },
        reset() {
            this.stop()
            this.time = 0
            this.secondes = 0
            this.minutes = 0
        },
        gatherResource()
        {
            if(this.gathering)
            {
            }
            this.gathering = !this.gathering;
        }
    },

    template: '<button class="btn btn-primary"   ' +

        //'v-bind:disabled="gathering" ' +
        //'v-on:click=" time = action.cooldown, countdown(), $root.newLog(action.log), $root.gatherResource(action.resource) ">' +
        'v-on:click="gatherResource(), $root.newLog(action.log), $root.gatherResource(action.resource) ">' + //, $root.initMove()
        '<span v-if="!gathering">{{action.name}}</span>' +
        '<span v-if="gathering">Gathering {{action.resource}}</span>' +

        '</button>'
})


Vue.component('button-hunt', {
    data: function () {
        return {
            hunting: false,
            log: "You started hunting"
        }
    },
    props: {
        action: Object,
    },
    methods: {
        switchHunting()
        {
            if(!this.hunting)
            {
                console.log("Starting hunt");
                this.hunting = true;
                this.log = "You started hunting"
                Hunting(true);
            } else {
                console.log("Stopping hunt");
                this.log = "You stopped hunting";
                $("HuntData").html();
                this.hunting = false;
                Hunting(false);
            }
        },
        confirmHunt: function()
        {

            console.log("confirm hunt!")
            axios.post("HuntConfirm").then(r=> console.log(r));
        }
    }, template:
    '<div>' +
        '<button class="btn btn-primary"   ' +
        'v-on:click="switchHunting(), $root.newLog(log)" >' +
        '<span v-if="!hunting">Start hunt</span v-if>' +
        '<span v-else>Stop hunt</span v-else>' +
        '</button> <p v-if="hunting">You are searching for animals...</p>' +
    '</div>'

});


Vue.component('button-craft', {
    data: function () {
        return {
            gathering: false,
            time: 0,
            timer: null,
            timerOn: false
        }
    },

    props: {
        item: Object,

    },
    items: [
        {
            name: "Axe",
            x: 0,
            cost: "2 Wood, 2 Stone"
        },
        {
            name: "Pickaxe",
            x: 0,
            cost: "2 Wood, 2 Stone"
        },
    ],
    methods: {
        craftItem()
        {
            this.$root.$emit("startProgress");

        }
    },

    template: '<button class="btn-danger"'+
        ' v-on:click="craftItem(),$root.craftItem(item.name)" >'+
        '<span>Craft {{item.name}}</span>'+
        '</button>',
})



document.addEventListener("DOMContentLoaded", function(event) {

    

    const {Splitpanes, Pane} = splitpanes
    
    var app = new Vue({
        el: '#app',
        


        components: {Splitpanes, Pane},
        data: {

            hideAll: false,
            hideLog: false,
            hideActions: false,
            hidePlayerResources: false,
            hideChat: true,
            hideMap: true,
            hideCrafting: true,
            hideTribe: true,

            date: new Date().toLocaleTimeString(),
            progWidth: "",
            progressing: false,
            interval: null,

            userName: "",
            userMessage:"",
            messages: [],
            
            actions: [

                {id: 1, name: "Gather Wood", log: "You gather some wood", cooldown: "15", resource:"Wood"},
                {id: 2, name: "Gather Stone", log: "You gather some stone", cooldown: "25",resource: "Stone"},
                {id: 3, name: "Gather Water", log: "You gather some water", cooldown: "20",resource: "Water"},
                {id: 4, name: "Gather Berries",log: "You gahter some berries", cooldown: "20",resource: "Berries"},
                {id: 5, name: "action5",},
                {id: 6, name: "action6",},
                {id: 7, name: "action7",},
                {id: 8, name: "action8",},
                {id: 9, name: "action9",},
                {id: 10, name: "action10",},
                {id: 11, name: "Craft Axe", log: "You crafted an axe", cooldown: "15", item:"Axe"},
                {id: 12, name: "Craft Pickaxe", log: "You crafted a pickaxe", cooldown: "15", item:"Pickaxe"},



            ],
            resources: [
                {
                    name: "Wood",
                    x: 0
                },
                {
                    name: "Stone",
                    x: 0
                },
                {
                    name: "Berries",
                    x: 0
                },
                {
                    name: "Water",
                    x: 0
                },
                {
                    name: "Food",
                    x: 0
                },
            ],

            items: [
                {
                    name: "Axe",
                    x: 0,
                    cost: "2 Wood, 2 Stone"
                },
                {
                    name: "Pickaxe",
                    x: 0,
                    cost: "2 Wood, 2 Stone"
                },

            ],

            logs: [
                {message: 'The journey starts now'},
                {message: 'Gather your wits about you'},
                {message: 'It is a new day'},
                {message: 'Welcome to Utopia'}

            ],


        },
        /* 
        *  created: function () {
             var self = this;
    
             
             axios.get('/api/resources')
                 .then(function (response) {
                     self.resources = response.data;
                 })
    
             
             axios.get('/api/actions')
                 .then(function (response) {
                     self.actions = response.data;
                 })
         },*/

        methods: {

            loadResources: function () {
                for (let i in this.resources) {
                    console.log(app.resources[i].name);
                    $.ajax({
                        url: "/Home/GetResource",

                        data: "resName=" + app.resources[i].name,
                        type: "GET",
                        success: function (data) {
                            app.resources[i].x = data;
                            console.log(data);
                        },
                        error: function (passParams) {
                            console.log(passParams)
                        }
                    });

                }
            },
            

            gatherResource: function (name) {
                console.log(name);
                $.ajax({
                    url: "/Home/ResGather",

                    data: "resourceName=" + name,
                    type: "POST",
                    success: function (data) {
                        console.log(data);
                    },
                    error: function (passParams) {
                        console.log(passParams)
                    }
                });
            },

            loadItems: function()
            {
                for(let i in this.items)
                {
                    console.log(app.items[i].name);
                    $.ajax({
                        url: "/Home/GetItem",

                        data: "itemName=" + app.items[i].name,
                        type: "GET",
                        success: function(data)
                        {
                            app.items[i].x = data;
                            console.log(data);
                        },
                        error: function(passParams)
                        {
                            console.log(passParams)
                        }
                    });

                }
            },

            craftItem(name) {
                console.log(name);
                $.ajax({
                    url: "/Home/ItemCraft",

                    data: "itemName=" + name,
                    type: "POST",
                    success: function (data) {
                        if(data === "Not enough")
                        {
                            alert("You do not have enough resources to craft this.");
                        }
                        console.log(data);
                    },
                    error: function (passParams) {
                        console.log(passParams)
                    }
                });
            },
            
            newLog: function (log) {
                var self = this;
                self.logs.push({message: log});

                console.log(log)

            },
            
            move: function () {
                let i = 1;
                let progWidth = 1;
                console.log(this.progWidth)
                let id = setInterval(frame, 10);

                function frame() {
                    console.log(this.progWidth)
                    if (progWidth >= 1000) {
                        console.log("clearing!")
                        progWidth = 0;
                    } else {
                        progWidth++;
                        this.progWidth = progWidth / 10 + "%";

                    }
                }


            },
            initMove() {
                console.log("Moving!");
                if (!this.progressing) {
                    this.move();
                    this.progressing = !this.progressing;
                } else {
                    this.progressing = !this.progressing;
                }

            },
            stopMove() {
                console.log("Stopping move!")
                this.progWidth = 0;
                clearInterval(self.move);
            },

            submitCard: function(user) {
                function submitData(event) {
                    event.preventDefault();

                }
                this.userName = user;
                if (this.userName && this.userMessage) {
                    // ---------
                    //  Call hub methods from client
                    // ---------
                    this.connection
                        .invoke("SendMessage", this.userName, this.userMessage)
                        .catch(function(err) {
                            return console.error(err.toSting());
                        });

                    this.userName = "";
                    this.userMessage = "";
                }
            }

        },
        created: function() {
            // ---------
            // Connect to our hub
            // ---------
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .configureLogging(signalR.LogLevel.Information)
                .build();
            document.getElementById("sendButton").disabled = true;

            this.connection.start().then(function () {
                document.getElementById("sendButton").disabled = false;
            }).catch(function(err) {
                return console.error(err.toSting());
            });
            
        },

        mounted: function() {
            // ---------
            // Call client methods from hub
            // ---------
/*            this.connection.on("ReceiveMessage", function (user, message) {
                var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
                if(msg.length!==0) {
                    var encodedMsg = user + ": " + msg;
                    var li = document.createElement("li");
                    li.textContent = encodedMsg;
                    document.getElementById("messagesList").appendChild(li);
                }
            });*/

            var thisVue = this;
            //thisVue.connection.start();
            thisVue.connection.on("Receive", function(user, message) {
                let date = new Date().toLocaleTimeString()
                thisVue.messages.push({ user, message, date });
            });
        }

    });

    
});

