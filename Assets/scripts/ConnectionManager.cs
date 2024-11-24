using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    List<ConnectionWaterBeet> connectionsWB = new List<ConnectionWaterBeet>();

    public void AddConnectionWB(int splineID, Beet beet, BucketBoys bucketBoys)
    {
        // Check if connection already exists, then add
        foreach (ConnectionWaterBeet connection in connectionsWB)
        {
            if (connection.splineID == splineID)
            {
                connection.beets.Add(beet);
                connection.bucketBoys.Add(bucketBoys);
                return;
            }
        }
        // If not, create new connection
        ConnectionWaterBeet newConnection = new ConnectionWaterBeet();
        newConnection.splineID = splineID;
        newConnection.beets.Add(beet);
        newConnection.bucketBoys.Add(bucketBoys);
        connectionsWB.Add(newConnection);
        
        CalculateWaterFlow();
    }

    public int GetWaterOutput(ConnectionWaterBeet connection)
    {
        int waterOutput = 0;

        foreach (BucketBoys bb in connection.bucketBoys)
        {
            waterOutput += bb.WaterOutput;
        }
        return waterOutput;
    }

    public int GetWaterConsumption(ConnectionWaterBeet connection)
    {
        int waterConsumption = 0;

        foreach (Beet beet in connection.beets)
        {
            waterConsumption += beet.GetWaterDemand();
        }
        return waterConsumption;
    }

    public void CalculateWaterFlow()
    {
        foreach (ConnectionWaterBeet connection in connectionsWB)
        {
            int waterOutput = GetWaterOutput(connection);
            int waterConsumption = GetWaterConsumption(connection);

            foreach (Beet beet in connection.beets)
            {
                 beet.enoughWater = waterOutput >= waterConsumption;
            }

        }
    }
}

public class ConnectionWaterBeet
{
    public int splineID;
    public List<Beet> beets = new List<Beet>();
    public List<BucketBoys> bucketBoys = new List<BucketBoys>();
}
