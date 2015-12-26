﻿using MongoDB.Driver;
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

        public User Post(Packet p)
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
                case "UpdateUser":
                    return UpdateUser(p);
                    break;
            }

            return null;
        }

        User GetUser(Packet p)
        {
            IMongoCollection<User> collection = _database.GetCollection<User>("userinfos");
            var user = collection.Find<User>(x => x._id == p.User._id).ToListAsync().GetAwaiter().GetResult();
            User userInfo = new User();
            userInfo._id = user[0]._id;
            userInfo.username = user[0].username;
            userInfo.description = user[0].description;
            userInfo.dateJoined = user[0].dateJoined;
            return userInfo;
        }


        User UpdateUser(Packet p)
        {
            IMongoCollection<User> collection = _database.GetCollection<User>("userinfos");
            var user = collection.Find<User>(x => x._id == p.User._id).ToListAsync().GetAwaiter().GetResult();

            var listInfo = new List<User>{
                             new User { description = p.User.description,
                                        profilePicture = p.User.profilePicture
                                      }
                };
            var filter = Builders<User>.Filter.Eq("_id", p.User._id);
            var updateDescription = Builders<User>.Update.Set("description", p.User.description);
            var updateProfilePicture = Builders<User>.Update.Set("profilePicture", p.User.profilePicture);
            collection.UpdateOneAsync(filter, updateDescription).GetAwaiter().GetResult();
            collection.UpdateOneAsync(filter, updateProfilePicture).GetAwaiter().GetResult();
            return null;
        }
        
        
        User Login(Packet p)
        {
            //Data query from DB
            IMongoCollection<UserCredential> collection = _database.GetCollection<UserCredential>("usercreds");
            var user = collection.Find<UserCredential>(x => x.username == p.UserCreds.username).ToListAsync().GetAwaiter().GetResult();

            if (user.Count != 0) { // >0 if there is a username in database that already registered 
                //Data that user entered
                var list = new List<UserCredential>{
                                 new UserCredential { _id = p.UserCreds._id,
                                            username = p.UserCreds.username,
                                            password = p.UserCreds.password
                                            }
                               };

                //Check if password is also true
                if (list[0].password == user[0].password)
                {
                    var testClass = new User()
                    {
                        username = p.UserCreds.username
                    };
                    var pack = new Packet()
                    {
                        Function = "GetUser",
                        User = testClass
                    };
                    return GetUser(pack);
                }
                else {
                    //p.Error.error = "{\"error\": \"Invalid Username - Password combination.\"}";
                    //return errorPacket;
                    return null;
                }
            }
            else //No username like that
            {
                //p.Error.error = "{\"error\": \"There is no registered username.\"}";
                //return errorPacket;
                return null;
            }
            
        }
        

        User SignUp(Packet p)
        {
            IMongoCollection<UserCredential> collection = _database.GetCollection<UserCredential>("usercreds"); //Connection to usercreds collection

            //Check if the username already exists
            var userCred = collection.Find<UserCredential>(x => x.username == p.UserCreds.username).ToListAsync().GetAwaiter().GetResult();
            if (userCred.Count > 0)
            {
                //User errorPacket = new User();
                //p.User.error = "{\"error\": \"Username already exists.\"}";
                //return errorPacket;
            }
            else //If not, get the username and pass from received packet and insert into usercreds collection in database
            {
                var listCred = new List<UserCredential>{
                             new UserCredential { _id = p.UserCreds._id,
                                        username = p.UserCreds.username,
                                        password = p.UserCreds.password
                             }
                 };
                collection.InsertManyAsync(listCred).GetAwaiter().GetResult(); //creadentials'i insert ettik


                //After sign up, we will initialize user collections in database
                IMongoCollection<User> collectionUser = _database.GetCollection<User>("userinfos");
                var listInfo = new List<User>{
                             new User { _id = p.UserCreds._id,
                                        username = p.UserCreds.username,
                                        description = "",
                                        dateJoined = DateTime.Now.ToOADate(),
                                        profilePicture = ""
                                      }
                };
                collectionUser.InsertManyAsync(listInfo).GetAwaiter().GetResult();
            }
            return null;
        }


        User SendTweet(Packet p)
        {
            IMongoCollection<Tweet> collectionTweets = _database.GetCollection<Tweet>("usertweets"); //Connection to usercreds collection

            var tweetList = new List<Tweet>{
                        new Tweet { _id = p.Tweet._id,
                                    dateTimePosted = DateTime.Now.ToOADate(),
                                    tweet = p.Tweet.tweet
                                  }
            };

            //var tweetsList = new List<Tweets>{
            //           new Tweets {
            //                        tweets = tweetList
            //                      }
            //};
            collectionTweets.InsertManyAsync(tweetList).GetAwaiter().GetResult();
            //}
            return null;
        }

        /*
        User SendTweet(Packet p)
        {
            IMongoCollection<User> collection = _database.GetCollection<User>("userinfos");
            //Check for any problem
            //Tweet CurrentTweet = JsonConvert.DeserializeObject<Tweet>(p.tweets);

            
            var user = collection.Find<User>(x => x._id == p.User._id).ToListAsync().GetAwaiter().GetResult();
            if (user.Count == 0)
            {
                User errorPacket = new User();
                p.User.error = "{\"error\": \"An error occured while receiving your data.\"}";
                return errorPacket;
            }
            else //If not, get the _id and tweet from received packet and insert into database
            {
                //First get the current tweets of the user
                var us = collection.Find<User>(x => x._id == p.User._id).ToListAsync().GetAwaiter().GetResult();
                User current = new User();
                current.tweets = us[0].tweets;


                /*

                if (current.tweets != null) { 
                    string allTweetsStr = current.tweets.ToString();
                    allTweetsStr = allTweetsStr.Remove(allTweetsStr.Length - 1);
                    allTweetsStr = allTweetsStr + ", " + "{" + "\"tweet\"" + ":" + "\"" + p.tweets + "\"" + ", " + "\"datePosted\"" + ": " + "\"" + DateTime.Now.ToString("yyyy/MM/dd") + "\"" + "," + "\"timePosted\"" + ": " + "\"" + DateTime.Now.TimeOfDay.ToString() + "\"}" + "]";
                
                    User newTweet = new User();
                    newTweet.tweets = allTweetsStr;

                    var filter = Builders<User>.Filter.Eq("_id", p._id);
                    var update = Builders<User>.Update.Set("tweets", allTweetsStr);
                    var result = collection.UpdateOneAsync(filter, update).GetAwaiter().GetResult();

                    //Return the new string
                    return newTweet;
                }
                else
                {
                    //string allTweetsStr = current.tweets.ToString();
                    string allTweetsStr = "";
                    allTweetsStr = allTweetsStr + "[" + "{" + "\"tweet\"" + ":" + "\"" + p.tweets + "\"" + ", " + "\"datePosted\"" + ": " + "\"" + DateTime.Now.ToString("yyyy/MM/dd") + "\"" + "," + "\"timePosted\"" + ": " + "\"" + DateTime.Now.TimeOfDay.ToString() + "\"}" + "]";
                
                    var filter = Builders<User>.Filter.Eq("_id", p._id);
                    var update = Builders<User>.Update.Set("tweets", allTweetsStr);
                    var result = collection.UpdateOneAsync(filter, update).GetAwaiter().GetResult();

                    User newTweet = new User();
                    newTweet.tweets = allTweetsStr;

                    return newTweet;
                }*/
 //           }
   //         return null;
     //   }

        /*
        User GetUser(Packet p)
        {
            IMongoCollection<User> collection = _database.GetCollection<User>("userinfos");
            var user = collection.Find<User>(x => x._id == p._id).ToListAsync().GetAwaiter().GetResult();
            User userInfo = new User();
            userInfo.username = user[0].username;
            userInfo._id = user[0]._id;
            userInfo.description = user[0].description;
            userInfo.dateJoined = user[0].dateJoined;
            userInfo.profilePicture = user[0].profilePicture;
            userInfo.coverPicture = user[0].coverPicture;
            userInfo.tweets = user[0].tweets;
            userInfo.following = user[0].following;
            userInfo.followers = user[0].followers;
            return userInfo;
        }


        void InsertUser(Packet p)
        {
            IMongoCollection<User> collection = _database.GetCollection<User>("userinfos");
            var list = new List<User> 
                          {
                             new User { _id = p._id,
                                        username = p.username,
                                        description = p.descrition,
                                        dateJoined = DateTime.Now.ToString("yyyy/MM/dd"),
                                        profilePicture = p.profilePicture,
                                        coverPicture = p.coverPicture,
                                        tweets = p.tweets,
                                        following = p.following,
                                        followers = p.followers,
                                        }
                          };
            collection.InsertManyAsync(list).GetAwaiter().GetResult();
        }


        
        void InsertUser(Packet p)
        {
            IMongoCollection<User> collection = _database.GetCollection<User>("userinfos");
            var list = new List<User> 
                          {
                             new User { userID = p.userID, username = p.username }
                          };
            collection.InsertManyAsync(list).GetAwaiter().GetResult();
        }


        void UpdateUser(Packet p)
        {
            IMongoCollection<User> collection = _database.GetCollection<User>("ListDeneme");
            var filter = Builders<User>.Filter.Eq("id", p.ID);
            var update = Builders<User>.Update
                .Set("name", p.Name);
            //.CurrentDate("lastModified");
            var result = collection.UpdateOneAsync(filter, update).GetAwaiter().GetResult();
        }

        void DeleteUser(Packet p)
        {
            IMongoCollection<User> collection = _database.GetCollection<User>("ListDeneme");
            var filter = Builders<User>.Filter.Eq("id", p.ID);
            var result = collection.DeleteManyAsync(filter).GetAwaiter().GetResult();
        }
        */
    }
}