using Microsoft.Data.SqlClient;
using Roommates.Models;
using System.Collections.Generic;
using System;


namespace Roommates.Repositories
{
    /// <summary>
    ///  This class is responsible for interacting with Room data.
    ///  It inherits from the BaseRepository class so that it can use the BaseRepository's Connection property
    /// </summary>
    public class RoommateRepository : BaseRepository
    {
        /// <summary>
        ///  When new RoomRespository is instantiated, pass the connection string along to the BaseRepository
        /// </summary>
        /// Nothing else has to be done in the below method soooooothe brackets can close
        public RoommateRepository(string connectionString) : base(connectionString) { }

        /// <summary>
        ///  Get a list of all Rooms in the database
        /// </summary>
        public List<Roommate> GetAll()
        {
            //  We must "use" the database connection.
            //  Because a database is a shared resource (other applications may be using it too) we must
            //  be careful about how we interact with it. Specifically, we Open() connections when we need to
            //  interact with the database and we Close() them when we're finished.
            //  In C#, a "using" block ensures we correctly disconnect from a resource even if there is an error.
            //  For database connections, this means the connection will be properly closed.
            using (SqlConnection conn = Connection)
            {
                // Note, we must Open() the connection, the "using" block doesn't do that for us.
                conn.Open();

                // We must "use" commands too.
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Here we setup the command with the SQL we want to execute before we execute it.
                    cmd.CommandText = "SELECT Id, FirstName, LastName, RentPortion, MovedInDate, RoomId FROM Roommate";

                    // Execute the SQL in the database and get a "reader" that will give us access to the data.
                    SqlDataReader reader = cmd.ExecuteReader();

                    // A list to hold the rooms we retrieve from the database.
                    List<Roommate> roommates = new List<Roommate>();

                    // Read() will return true if there's more data to read
                    while (reader.Read())
                    {
                        // The "ordinal" is the numeric position of the column in the query results.
                        //  For our query, "Id" has an ordinal value of 0 and "Name" is 1.
                        int idColumnPosition = reader.GetOrdinal("Id");

                        // We user the reader's GetXXX methods to get the value for a particular ordinal.
                        int idValue = reader.GetInt32(idColumnPosition);

                        int firstNameColumnPosition = reader.GetOrdinal("FirstName");
                        string firstNameValue = reader.GetString(firstNameColumnPosition);

                        int lastNameColumnPosition = reader.GetOrdinal("LastName");
                        string lastNameValue = reader.GetString(lastNameColumnPosition);

                        int rentPortionColumnPosition = reader.GetOrdinal("RentPortion");
                        int rentPortionValue = reader.GetInt32(rentPortionColumnPosition);

                        int moveInDateColumnPosition = reader.GetOrdinal("MoveInDate");
                        DateTime movedInDateValue = reader.GetDateTime(moveInDateColumnPosition);

                        //int roommateIdColumnPosition = reader.GetOrdinal("RoommateId");

                        // Now let's create a new room object using the data from the database.
                        Roommate roommate = new Roommate
                        {
                            Id = idValue,
                            Firstname = firstNameValue,
                            Lastname = lastNameValue,
                            RentPortion = rentPortionValue,
                            MovedInDate = movedInDateValue,
                            Room = null

                        };

                        // ...and add that room object to our list.
                        roommates.Add(roommate);
                    }

                    // We should Close() the reader. Unfortunately, a "using" block won't work here.
                    reader.Close();

                    // Return the list of rooms who whomever called this method.
                    return roommates;
                }
            }
        }
        /// <summary>
        ///  Returns a single room with the given id.
        /// </summary>
        public Roommate GetById(int id)
        { 
            ///Base repos. keeps the info on connection
            using (SqlConnection conn = Connection)
            {
                conn.Open();
               
                using (SqlCommand cmd = conn.CreateCommand())
                    
                {
                    ///via instruction session i have added RoomId and ID to the select
                    cmd.CommandText = "SELECT rm.Id, rm.LastName,  rm.FirstName, rm.RentPortion, rm.MovedInDate, rm.RoomId AS RoommateRoomId, r.id AS RoomId, r.Name, r.MaxOccupancy  FROM Roommate rm LEFT JOIN Room r ON r.id = rm.RoomId WHERE rm.id = @id";
                    ///This cmd happens in the database 
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    Roommate roommate = null;
                   // Room room = null;

                    // If we only expect a single row back from the database, we don't need a while loop.
                    if (reader.Read())
                    {
                        ///Have to bring in room and set the new object so that we can tie it into our CRUD management 
                        Room room = new Room()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("RoomId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            MaxOccupancy = reader.GetInt32(reader.GetOrdinal("MaxOccupancy"))

                        };
                        roommate = new Roommate()
                        {
                            Id = id,
                            Firstname = reader.GetString(reader.GetOrdinal("FirstName")),
                            Lastname = reader.GetString(reader.GetOrdinal("LastName")),
                            RentPortion = reader.GetInt32(reader.GetOrdinal("RentPortion")),
                            MovedInDate = reader.GetDateTime(reader.GetOrdinal("MoveInDate")),
                            Room = room
                        };
                    }

                    reader.Close();
                    return roommate;
                }
               // return null; (Dont need to do this because it is above, if false it will return null
            }
        }
        /// <summary>
        ///  Add a new room to the database
        ///   NOTE: This method sends data to the database,
        ///   it does not get anything from the database, so there is nothing to return.
        /// </summary>
        public void Insert(Roommate roommate)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())


                {
                    // These SQL parameters are annoying. Why can't we use string interpolation?
                    // ... sql injection attacks!!!
                    cmd.CommandText = @"INSERT INTO Roommate (FirstName, LastName, RentPortion, MovedInDate, RoomId) 
                                         OUTPUT INSERTED.Id 
                                         VALUES (@FirstName, @LastName, @RentPortion, @MovedInDate, @RoomId)";
                    //
                    cmd.Parameters.AddWithValue("@FirstName", roommate.Firstname);
                    cmd.Parameters.AddWithValue("@LastName", roommate.Lastname);
                    cmd.Parameters.AddWithValue("@RentPortion", roommate.RentPortion);
                    cmd.Parameters.AddWithValue("@MoveInDate", roommate.MovedInDate);
                    cmd.Parameters.AddWithValue("@RoomId", roommate.Room.Id);
                    int id = (int)cmd.ExecuteScalar();
                    //int roomId = ?
                    roommate.Id = id;
                }
            }

            // when this method is finished we can look in the database and see the new room.
        }
        /// <summary>
        ///  Updates the room
        /// </summary>
        //public void Update(Roommate roommate)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"UPDATE Roommate
        //                            SET
        //                            FirstName = @firstName,
        //                            LastName = @lastName,
        //                            RentPortion = @rentPortion,
        //                            MovedInDate = @movedInDate,

        //                            WHERE Id = @id";

        //            cmd.Parameters.AddWithValue("@id", room.Id);
        //            cmd.Parameters.AddWithValue("@firstName", roommate.Firstname);
        //            cmd.Parameters.AddWithValue("@lastName", roommate.Lastname);
        //            cmd.Parameters.AddWithValue("@rentPortion", roommate.RentPortion);
        //            cmd.Parameters.AddWithValue("@movedInDate", roommate.MovedInDate);
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}
    }
}