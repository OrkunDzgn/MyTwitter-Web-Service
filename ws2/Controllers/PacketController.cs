using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ws2.Models;

namespace ws2.Controllers
{



    public class PacketController : ApiController
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;


        public PacketController()
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("TwitterDB"); //Database Name

        }

        public Packet Post(Packet p)
        {
            switch (p.Function)
            {
                case "GetUser":
                    return GetUser(p);
                    break;
                case "Login":
                    return Login(p);
                    break;
                case "SignUp":
                    return SignUp(p);
                    break;
                case "SendTweet":
                    return SendTweet(p);
                    break;
                case "GetTweets":
                    return GetTweets(p);
                    break;
                case "UpdateUser":
                    return UpdateUser(p);
                    break;
            }

            return null;
        }

        Packet GetUser(Packet p)
        {
            Packet pSend = new Packet();
            IMongoCollection<User> collection = _database.GetCollection<User>("userinfos");
            var user = collection.Find<User>(x => x.username == p.User.username).ToListAsync().GetAwaiter().GetResult();

            if (user.Count > 0) { 
                var testClass = new User()
                {
                    _id = user[0]._id,
                    username = user[0].username,
                    dateJoined = user[0].dateJoined,
                    description = user[0].description,
                    profilePicture = user[0].profilePicture
                };
                var errorClass = new Error()
                {
                    error = false,
                    errorDescription = "User informations received."
                };
                var testPacket = new Packet()
                {
                    User = testClass,
                    Error = errorClass
                };

                return testPacket; //Return packet with user informations
            }
            else
            {
                var errorClass = new Error()
                {
                    error = true,
                    errorDescription = "Username couldn't be found."
                };
                var testPacket = new Packet()
                {
                    Error = errorClass
                };

                return testPacket; //Send Packet with Error
            }
        }


        Packet UpdateUser(Packet p)
        {
            IMongoCollection<User> collection = _database.GetCollection<User>("userinfos");
            var user = collection.Find<User>(x => x.username == p.User.username).ToListAsync().GetAwaiter().GetResult();

            if (user.Count > 0)
            {
                var listInfo = new List<User>{
                             new User { description = p.User.description,
                                        profilePicture = p.User.profilePicture
                                      }
                };
                if (p.User.description != null && p.User.profilePicture != null)
                {
                    var filter = Builders<User>.Filter.Eq("username", p.User.username);
                    var updateDescription = Builders<User>.Update.Set("description", p.User.description);
                    //var updateProfilePicture = Builders<User>.Update.Set("profilePicture", p.User.profilePicture);
                    collection.UpdateOneAsync(filter, updateDescription).GetAwaiter().GetResult();
                    //collection.UpdateOneAsync(filter, updateProfilePicture).GetAwaiter().GetResult();
                    var errorClass = new Error()
                    {
                        error = false,
                        errorDescription = "User description and profile picture updated."
                    };
                    var testPacket = new Packet()
                    {
                        Error = errorClass
                    };
                    return testPacket;
                }
                else if (p.User.description != null) {
                    var filter = Builders<User>.Filter.Eq("username", p.User.username);
                    var updateDescription = Builders<User>.Update.Set("description", p.User.description);
                    //var updateProfilePicture = Builders<User>.Update.Set("profilePicture", p.User.profilePicture);
                    collection.UpdateOneAsync(filter, updateDescription).GetAwaiter().GetResult();
                    //collection.UpdateOneAsync(filter, updateProfilePicture).GetAwaiter().GetResult();
                    var errorClass = new Error()
                    {
                        error = false,
                        errorDescription = "User description updated."
                    };
                    var testPacket = new Packet()
                    {
                        Error = errorClass
                    };
                    return testPacket;
                }
                else if(p.User.profilePicture != null)
                {
                    var filter = Builders<User>.Filter.Eq("username", p.User.username);
                    //var updateDescription = Builders<User>.Update.Set("description", p.User.description);
                    var updateProfilePicture = Builders<User>.Update.Set("profilePicture", p.User.profilePicture);
                    //collection.UpdateOneAsync(filter, updateDescription).GetAwaiter().GetResult();
                    collection.UpdateOneAsync(filter, updateProfilePicture).GetAwaiter().GetResult();
                    var errorClass = new Error()
                    {
                        error = false,
                        errorDescription = "User profile picture updated."
                    };
                    var testPacket = new Packet()
                    {
                        Error = errorClass
                    };
                    return testPacket;
                }
                return null;
                
                
            }
            else
            {
                var errorClass = new Error()
                {
                    error = true,
                    errorDescription = "Username couldn't be updated."
                };
                var testPacket = new Packet()
                {
                    Error = errorClass
                };

                return testPacket; //Send Packet with Error
            }
        }
        
        
        Packet Login(Packet p)
        {
            //Data query from DB
            IMongoCollection<UserCredential> collection = _database.GetCollection<UserCredential>("usercreds");
            var user = collection.Find<UserCredential>(x => x.username == p.UserCreds.username).ToListAsync().GetAwaiter().GetResult();

            if (user.Count != 0) { // >0 if there is a username in database that already registered 
                //Data that user entered
                var list = new List<UserCredential>{
                                 new UserCredential { 
                                            _id = p.UserCreds._id,
                                            username = p.UserCreds.username,
                                            password = p.UserCreds.password
                                            }
                               };

                //Check if password is also true
                if (list[0].password == user[0].password)
                {
                    var testClass = new User()
                    {
                        _id = p.UserCreds._id,
                        username = p.UserCreds.username
                    };
                    var errorClass = new Error()
                    {
                        error = false,
                        errorDescription = "User Login Successful"
                    };
                    var pack = new Packet()
                    {
                        Function = "GetUser",
                        User = testClass
                    };
                    return GetUser(pack);
                }
                else { //Password wrong
                    var errorClass = new Error()
                    {
                        error = true,
                        errorDescription = "Password does not match with the username."
                    };
                    var testPacket = new Packet()
                    {
                        Error = errorClass
                    };

                    return testPacket; //Send Packet with Error
                }
            }
            else //Username does not exist in the database
            {
                var errorClass = new Error()
                {
                    error = true,
                    errorDescription = "Username does not exist."
                };
                var testPacket = new Packet()
                {
                    Error = errorClass
                };

                return testPacket; //Send Packet with Error
            }
            
        }
        

        Packet SignUp(Packet p)
        {
            IMongoCollection<UserCredential> collection = _database.GetCollection<UserCredential>("usercreds"); //Connection to usercreds collection

            //Check if the username already exists
            var userCred = collection.Find<UserCredential>(x => x.username == p.UserCreds.username).ToListAsync().GetAwaiter().GetResult();
            if (userCred.Count > 0)
            {
                var errorClass = new Error()
                {
                    error = true,
                    errorDescription = "Username already exists."
                };
                var testPacket = new Packet()
                {
                    Error = errorClass
                };

                return testPacket; //Send Packet with Error
            }
            else //If not, get the username and pass from received packet and insert into usercreds collection in database with random ID
            {
                var r = new Random();
                int userRandomID = r.Next(1, 2147483646);

                var listCred = new List<UserCredential>{
                             new UserCredential { _id = userRandomID,
                                        username = p.UserCreds.username,
                                        password = p.UserCreds.password
                             }
                 };
                collection.InsertManyAsync(listCred).GetAwaiter().GetResult(); //creadentials'i insert ettik


                //After sign up, we will initialize user collections in database
                IMongoCollection<User> collectionUser = _database.GetCollection<User>("userinfos");
                var listInfo = new List<User>{
                             new User { _id = userRandomID,
                                        username = p.UserCreds.username,
                                        description = "",
                                        dateJoined = DateTime.Now.ToOADate(),
                                        profilePicture = ""
                                      }
                };
                collectionUser.InsertManyAsync(listInfo).GetAwaiter().GetResult();
                var errorClass = new Error()
                {
                    error = false,
                    errorDescription = "Sign Up successful."
                };
                var testPacket = new Packet()
                {
                    Error = errorClass
                };

                return testPacket; //Send Packet with Error
            }
        }


        Packet SendTweet(Packet p)
        {
            //Create a uniqueID for tweet -walkaround for unique _id in mongoDB
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string tweetID = new string(Enumerable.Repeat(chars, 20).Select(s => s[random.Next(s.Length)]).ToArray());

            //Check if that user exists
            IMongoCollection<User> collection = _database.GetCollection<User>("userinfos");
            var user = collection.Find<User>(x => x.username == p.User.username).ToListAsync().GetAwaiter().GetResult();

            if (user.Count > 0) // If user exists, send tweet
            {
                IMongoCollection<Tweet> collectionTweets = _database.GetCollection<Tweet>("usertweets"); //Connection to usercreds collection

                var tweetList = new List<Tweet>{
                        new Tweet { 
                                    _id = tweetID,
                                    userID =  user[0]._id,
                                    username = p.Tweet.username,
                                    dateTimePosted = DateTime.Now.ToOADate(),
                                    tweet = p.Tweet.tweet
                                  }
                };

                collectionTweets.InsertManyAsync(tweetList).GetAwaiter().GetResult();

                var errorClass = new Error()
                {
                    error = false,
                    errorDescription = "Tweet sent."
                };
                var testPacket = new Packet()
                {
                    Error = errorClass
                };

                return testPacket; //Send Packet with Error
            }
            else //user doesn't exist, so cannot send tweet
            {
                var errorClass = new Error()
                {
                    error = false,
                    errorDescription = "Username register does not exist to send the tweet."
                };
                var testPacket = new Packet()
                {
                    Error = errorClass
                };

                return testPacket; //Send Packet with Error
            }
        }


        Packet GetTweets(Packet p)
        {
            Packet pSend = new Packet();
            IMongoCollection<Tweet> collection = _database.GetCollection<Tweet>("usertweets");
            var user = collection.Find<Tweet>(x => x.username == p.Tweet.username).ToListAsync().GetAwaiter().GetResult();
            if (user.Count > 0) { 
                List<Tweet> tweetsList = new List<Tweet>();

                for (int i = 0; i < user.Count; i++)
                {
                    tweetsList.Add(new Tweet {
                                              _id = user[0]._id,
                                              userID = user[0].userID,
                                              username = user[0].username, 
                                              tweet = user[i].tweet,
                                              dateTimePosted = user[i].dateTimePosted
                                             }
                    );
                }
                var testClass = new Tweets()
                {
                    tweets = tweetsList
                };
                var errorClass = new Error()
                {
                    error = false,
                    errorDescription = "Tweets received."
                };
                var pack = new Packet()
                {
                    Error = errorClass,
                    Tweets = testClass
                };

                return pack;
            }
            else
            {
                var errorClass = new Error()
                {
                    error = false,
                    errorDescription = "No tweets sent by " + p.Tweet.username
                };
                var testPacket = new Packet()
                {
                    Error = errorClass
                };

                return testPacket; //Send Packet with Error
            }
        }
    }
}