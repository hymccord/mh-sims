#Copyright (c) 2021 HappyFeetMM
#All rights reserved.

#This source code is licensed under the BSD-style license found in the
#LICENSE file in the root directory of this source tree. 
#!/usr/bin/env python
# coding: utf-8

from multiprocessing import Pool
import sys
import numpy as np
import random as rd
#Un-comment this if you want histograms, as well as one other line at the bottom of the VRift class
#import matplotlib.pyplot as plt
import pandas as pd
import time

#This class is basically a niche tsitu's CRE
class MiceData:
    #Mice AR for different "levels" of tower, as well as UU
    miceDict={}
    #All values taken from CRE
    miceDict[("Low",False)]=[0.,0.22,0.,0.14,0.,0.,0.04,0.03,0.57,0.]
    miceDict[("Medium",False)]=[0.,0.12,0.,0.09,0.,0.28,0.03,0.02,0.36,0.10]
    miceDict[("High",False)]=[0.,0.11,0.,0.08,0.12,0.13,0.03,0.02,0.33,0.18]
    miceDict[("Highest",False)]=[0.,0.10,0.11,0.07,0.07,0.04,0.02,0.02,0.29,0.28]
    miceDict[("ExtraUU1",False)]=[0.,0.10,0.11,0.07,0.07,0.04,0.02,0.02,0.29,0.28]#Same as highest
    miceDict[("ExtraUU2",False)]=[0.,0.10,0.11,0.07,0.07,0.04,0.02,0.02,0.29,0.28]#Same as highest
    
    #UU Data added thanks to Aaron/Xalca
    miceDict[("Low",True)]=[0.1827,0.1819,0.,0.1169,0.,0.,0.0412,0.0220,0.4553,0.]
    miceDict[("Medium",True)]=[0.1875,0.1106,0.,0.0729,0.,0.2284,0.0215,0.0149,0.2897,0.0745]
    miceDict[("High",True)]=[0.1935,0.0996,0.,0.0658,0.1031,0.1084,0.0195,0.0139,0.2644,0.1318]
    miceDict[("Highest",True)]=[0.1963,0.0925,0.0915,0.0603,0.0622,0.0361,0.0173,0.0123,0.2453,0.1862]
    miceDict[("ExtraUU1",True)]=[0.2016,0.0856,0.0805,0.0568,0.0573,0.0376,0.0161,0.0100,0.2246,0.2299]
    miceDict[("ExtraUU2",True)]=[0.2009,0.0795,0.0793,0.0532,0.0539,0.0322,0.0158,0.0103,0.2118,0.2631]
    miceDict[("Eclipse",False)]=[1]
    miceDict[("Eclipse",True)]=[1]
        
    #For generic mice, not floor specific
    micePowers=[818250,100,350000,4800,150000,38000,8250,23000]
    miceEff=[7500,150,5000,175,2500,1000,175,175]
    
    #Generic names
    names=["Bulwark of Ascent",
           "Terrified Adventurer",
           "Soldier Of The Shade",
           "Unwavering Adventurer",
           "Prestigious Adventurer",
           "Possessed Armaments",
           "Berzerker",
           "Lumi-Lancer"]
    
    #Floor specific additions
    floorPowers={"Puppet":[2900,72000],
                 "Thief":[6650,72000],
                 "Melee":[8800,72000],
                 "Bard":[11750,72000],
                 "Magic":[16000,72000],
                 "Noble":[21500,72000],
                 "Dusty":[29000,72000]}
    
    floorEff={"Puppet":[100,900],
                 "Thief":[200,900],
                 "Melee":[300,900],
                 "Bard":[400,900],
                 "Magic":[500,900],
                 "Noble":[600,900],
                 "Dusty":[700,900]}
    
    floorNames={"Puppet":["Puppetto","Puppet Champion"],
                 "Thief":["Cutpurse","Champion Thief"],
                 "Melee":["Martial","Praetorian Champion"],
                 "Bard":["One-Mouse Band","Champion Danseuse"],
                 "Magic":["Mouse of Elements","Magic Champion"],
                 "Noble":["Cursed Crusader","Fallen Champion Footman"],
                 "Dusty":["Withered Remains","Arch Champion Necromancer"]}
    
    #Power/eff for eclipse mice
    eclipseData={"Shade":[7000000,100000],"Total":[13500000,100000]}
    
    #Function to figure out which AR group to pull data from
    def getFloorInfo(floor):
        #floorRange determines which tower height AR pool to select
        if(0<floor<=7):
            floorRange="Low"
        if(8<floor<=15):
            floorRange="Medium"
        if(16<floor<=23):
            floorRange="High"
        if(24<floor<=47):
            floorRange="Highest"
        if(48<floor<=63):
            floorRange="ExtraUU1"
        if(64<floor):
            floorRange="ExtraUU2"
        #floorType is Puppet, Thief, Melee etc.
        floorTypes=MiceData.floorPowers.keys()
        for i,ftype in enumerate(floorTypes):
            if(floor%8==i+1):
                floorType=ftype
        if(floor%8==0):
            floorType="Eclipse"
            floorRange="Eclipse"
        return floorType,floorRange
    #tsitu's CR formula, taken straight from the github page: https://github.com/tsitu/MH-Tools#-new-formula
    def CR(E,T,L,M):
        ML=np.ceil(np.ceil(np.sqrt(M/2))/min(1.4,E))
        if(L>ML):
            return 1
        else:
            return (E*T+2*(np.floor(min(1.4,E)*L))**2)/(E*T+M)
        
    #collects data on the floor that is passed to this function: Floor Type, AR, CR and names
    def getCurrentData(floor,UU,trapPower,trapLuck):
        floorType,floorRange=MiceData.getFloorInfo(floor)
        
        currentAR=MiceData.miceDict[(floorRange,UU)]
        currentPowers=MiceData.micePowers+MiceData.floorPowers[floorType]
        currentEff=MiceData.miceEff+MiceData.floorEff[floorType]
        currentNames=MiceData.names+MiceData.floorNames[floorType]
        
        currentCR=[]
        
        for i in range(len(currentAR)):
            E=currentEff[i]/100#convert from % to proportion
            M=currentPowers[i]
            T=trapPower
            L=trapLuck
            currentCR.append(MiceData.CR(E,T,L,M))
        
        return floorType,currentAR,currentCR,currentNames
    
    #Special function for eclipse data (just CR is necessary)
    def getEclipseCR(UU,trapPower,trapLuck):
        if(UU==True):
            data=MiceData.eclipseData["Total"]
        if(UU==False):
            data=MiceData.eclipseData["Shade"]
            
        M=data[0]
        E=data[1]/100
        T=trapPower
        L=trapLuck
        
        return MiceData.CR(E,T,L,M)
            
#This class handles the VRift skills, augmentations, run mechanics etc.
class VRift:
    def __init__(self, argv):
        #Bunch of inputs for easy user input
        
        numArgs = len(argv)
        #Integer inputs
        if (numArgs < 2):
            print("Trap Power?")
            self.trapPower=int(input())
        else :
            self.trapPower=int(argv[1])

        if (numArgs < 3):
            print("Trap Luck?")
            self.trapLuck=int(input())
        else:
            self.trapLuck=int(argv[2])

        if (numArgs < 4):
            print("Current Step?")
            self.currentStep=int(input())
        else:
            self.currentStep=int(argv[3])

        #print("Current Floor")
        #self.currentFloor=int(input())

        if (numArgs < 5):
            print("Speed?")
            self.Speed=int(input())
        else:
            self.Speed=int(argv[4])

        if (numArgs < 6):
            print("Sync?")
            self.Sync=int(input())
        else:
            self.Sync=int(argv[5])

        if (numArgs < 7):
            print("Siphon?")
            self.Siphon=int(input())
        else:
            self.Siphon=int(argv[6])
        
        #Boolean inputs
        if (numArgs < 8):
            print("CF?")
            self.CF=bool(int(input()))
        else:
            self.CF=bool(int(argv[7]))

        if(self.CF==1):
            self.Speed+=1
            
        if (numArgs < 9):
            print("Super Siphon?")
            superSiph=bool(int(input()))
        else:
            superSiph=bool(int(argv[8]))

        if(superSiph==1):
            self.Siphon=self.Siphon*2
        
        if (numArgs < 10):
            print("String Stepping?")
            self.StringStepping=bool(int(input()))
        else:
            self.StringStepping=bool(int(argv[9]))
        
        if (numArgs < 11):
            print("Ultimate Umbra?")
            self.UU=bool(int(input()))
        else:
            self.UU=bool(int(argv[10]))
        
        if (numArgs < 12):
            print("Ultimate Charm Eclipse?")
            self.UCEclipse=bool(int(input()))
        else:
            self.UCEclipse=bool(int(argv[11]))

        #For verbosity
        if (numArgs < 13):
            print("Detail?")
            self.Detail=bool(int(input()))
        else:
            self.Detail=bool(int(argv[12]))

        if (numArgs < 14):
            print("UC Hunts?")
            self.UCHuntsLeft2=int(input())
        else:
            self.UCHuntsLeft2=int(argv[13])
        
        self.TACount=0
        self.BulwarkCount=0
        self.UCHuntsLeft=self.UCHuntsLeft2
        
    #Makes an object for the mini-CRE above  
    mouseBank=MiceData
    
    #calculates steps, given a mouse catch
    def calcSteps(self,mouse,caught):
        if(mouse=="Eclipse"):
            if(caught):
                return self.Speed
            else:
                return 0
        if(mouse=="Bulwark of Ascent"):
            if(caught):
                return self.Speed
            else:
                self.BulwarkCount+=1
                return -10
        if(mouse=="Terrified Adventurer"):
            self.TACount+=1
            if(caught):
                if(self.StringStepping==True):
                    return self.Speed*4
                if(self.StringStepping==False):
                    return self.Speed*2
            else:
                return 0
        else:
            if(caught):
                return self.Speed
            else:
                if(self.UU):
                    return -5
                elif(self.UU==False):
                    return 0
    #updates the steps in the run
    #Also handles niche cases, like if bulwark can't push you back the full 10 steps because of the floor bottom
    #Or if you don't get a full catch worth of steps because you've reached the eclipse floor
    def updateSteps(self,steps):
        currFloor=self.calculateFloor(self.currentStep)
        nextStep=self.currentStep+steps
        
        #Handles case where bulwark is missed as 1st/2nd mouse
        if(nextStep<0):
            nextStep=0
        nextFloor=self.calculateFloor(nextStep)
        
        #Handles case where bulwark would otherwise push down a floor
        while(self.calculateFloor(nextStep)<currFloor):
            nextStep+=1
        
        #Handles case where the eclipse floor is reached mid-hunt
        if(nextFloor-currFloor>1):
            for i in range(currFloor+1,nextFloor):
                if(i%8==0):
                    while(self.calculateFloor(nextStep)>i):
                        nextStep-=1
                    continue
        steps=nextStep-self.currentStep
        if(self.Detail):
            print("Taking ",steps," Steps")
        self.currentStep+=steps
    #calculates the current floor based on the current step
    def calculateFloor(self,currStep):
        if(currStep==0):
            return 1
        lowerThreshold=0
        #Iterating over 100 eclipses (loop terminates once the correct floor is reached)
        for level in range(100):
            #Logic to keep adding 10 steps per 8 floors
            upperThreshold=(20+level*10)*7+lowerThreshold
            if(lowerThreshold<currStep<=upperThreshold):
                currFloor=int(np.floor((currStep-lowerThreshold)/(20+level*10))+1+level*8)
                return currFloor
            #Once 8 floors are moved past, the threshold updates if necessary (or the current floor is reached)
            lowerThreshold=upperThreshold  
            
    #Hunt logic for eclipse
    def HuntEclipse(self):
        if(self.Detail):
            if(self.UU):
                print("Total Eclipse")
            else:
                print("Shade Of The Eclipse")
            
        #If UC is used on Eclipse, catch immediately
        if(self.UCEclipse):
            caught=True
            if(self.Detail):
                print("Caught")
            return caught
        
        cr=self.mouseBank.getEclipseCR(self.UU,self.trapPower,self.trapLuck)
        rollCR=rd.uniform(0,1)
        
        if(rollCR>=cr):
            if(self.Detail):
                print("Missed")
            caught=False
        else:
            if(self.Detail):
                print("Caught")
            caught=True
        return caught
    
    #Hunt logic for other mice
    def HuntNormal(self):
        currFloor = self.calculateFloor(self.currentStep)
        miceCalcs=self.mouseBank.getCurrentData(currFloor,self.UU,self.trapPower,self.trapLuck)
        miceNames=miceCalcs[3]
        rollAR=rd.uniform(0,1)
        
        counter=0
        for i,mouseAR in enumerate(miceCalcs[1]):
            if(mouseAR==0.):
                continue
            counter+=mouseAR
            if(counter>rollAR):
                found=i
                mouse=miceNames[found]
                if(self.Detail):
                    print(mouse)
                break
                
        rollCR=rd.uniform(0,1) 
        foundCR=miceCalcs[2][found]
        
        if (self.UCHuntsLeft > 0):
            self.UCHuntsLeft -= 1
            if (self.Detail):
                print("UCHunt")
            caught=True
        elif (rollCR>=foundCR):
            if(self.Detail):
                print("Missed")
            caught=False
        else:
            if(self.Detail):
                print("Caught")
            caught=True
            
        return mouse,caught
    
    #Function to start the run
    def beginRun(self, num):
        startingStep=self.currentStep
        #Seeding rng
        rd.seed(time.time())
        huntsRemaining=self.Sync
        totalHunts=0
        self.TACount=0
        self.BulwarkCount=0
        self.UCHuntsLeft=self.UCHuntsLeft2
        while(huntsRemaining>0):
            currFloor=self.calculateFloor(self.currentStep)
            if(self.Detail):
                print("Floor ",currFloor)
                print("Step ",self.currentStep)
                print("Hunts Remaining: ",huntsRemaining)
            if(currFloor%8==0):
                caught=self.HuntEclipse()
                if(caught==True):
                    #Siphoning Eclipse
                    huntsRemaining+=self.Siphon
                steps=self.calcSteps("Eclipse",caught)
                self.updateSteps(steps)
            if(currFloor%8!=0):
                mouse,caught=self.HuntNormal()
                steps=self.calcSteps(mouse,caught)
                self.updateSteps(steps)
                
            totalHunts+=1
            huntsRemaining-=1
            
            if(self.Detail):
                print("----------------------------------")
                
        finalFloor=self.calculateFloor(self.currentStep)
        eclipseNumber=np.floor(finalFloor/8)
        
        if(self.Detail):
            print("Final Result")
            print("Floor: ",finalFloor)
            print("Eclipse: ",eclipseNumber)
            
        self.currentStep=startingStep
        
        return finalFloor,eclipseNumber,totalHunts,self.TACount,self.BulwarkCount
    
    #Chance to reach specified floor or higher (works for Eclipses or Normal)
    def findPercentFloor(self,floor,data):
        return 100*len(data[data>=floor])/len(data)

    def collectData(self,runs):
        detail=self.Detail
        #Automatically sets verbosity to 0 (to avoid accidentally picking verbosity on 1000 hunts)
        self.Detail=False
        
        data=[]
        print("----------------------------------")
        print("Starting ",runs," VRift Runs...")
        print("----------------------------------")

        # Parallel processing
        if (True):
            with Pool() as pool:
                for result in pool.map(self.beginRun, range(runs)):
                    data.append(result)
        else:
            for i in range(runs):
                data.append(self.beginRun(i))
            
        #Back to normal
        self.Detail=detail
        
        print("Overall Results for ",runs, "Runs")
        
        df=pd.DataFrame(data)
        
        #HISTOGRAMS
        #Un-comment the code below if you want histograms of the data. 
        #First histogram will be the floor, next will be the Eclipse floor, last will be hunts 
        
        #df.hist()
        
        floorData=df[0]
        eclipseData=df[1]
        huntData=df[2]
        TAData=df[3]
        BulwarkData=df[4]
        
        meanFloor=floorData.mean()
        sdFloor=np.round(np.sqrt(floorData.var()),2)
        print("Average Floor Reached: ",meanFloor,"+/-",sdFloor)
        
        meanEclipse=eclipseData.mean()
        sdEclipse=np.round(np.sqrt(eclipseData.var()),2)
        print("Average Eclipse Reached: ",meanEclipse,"+/-",sdEclipse)
        
        meanHunts=huntData.mean()
        sdHunts=np.round(np.sqrt(huntData.var()),2)
        print("Average Hunts ",meanHunts,"+/-",sdHunts)
        
        minEclipse=int(eclipseData.min())
        maxEclipse=int(eclipseData.max())
        
        for eclipse in range(minEclipse,maxEclipse+1):
            conditional=" or higher: "
            if(eclipse==maxEclipse):
                conditional=""
            #Gets rows corresponding to current eclipse
            currentEclipse=df[eclipseData==eclipse]

            #Calculate means of TA/Bulwark Per Hunt
            meanTAPerHunt=np.round((currentEclipse[3]/currentEclipse[2]).mean(),3)
            meanBulwarkPerHunt=np.round((currentEclipse[4]/currentEclipse[2]).mean(),3)

            #Multiply per hunt values by mean of the minimum Eclipse floor
            minEclipseHunts=df[eclipseData==minEclipse][2].mean()

            #Data now normalised to the minimum Eclipse Floor hunts
            normTA=np.round(meanTAPerHunt*minEclipseHunts,1)
            normBulwark=np.round(meanBulwarkPerHunt*minEclipseHunts,1)

            print("Chance of Eclipse ",eclipse,conditional,self.findPercentFloor(eclipse,eclipseData),"%","Mean TAs (Normalised): ",normTA,"Mean Bulwarks Missed (Normalised): ",normBulwark)
        #SAME PROBABILITY ANALYSIS BUT FOR INDIVIDUAL FLOORS
        #minFloor=int(floorData.min())
        #maxFloor=int(floorData.max())
        
        #for floor in range(minFloor,maxFloor+1):
            #conditional=" or higher: "
            #if(floor==maxFloor):
                #conditional=""
                
            #print("Chance of Floor ",floor,conditional,self.findPercentFloor(floor,floorData),"%")
        return df

if __name__ == "__main__":
    tower=VRift(sys.argv)

    numRuns = 100
    if (len(sys.argv) > 12):
        numRuns = int(sys.argv[12])

    df=tower.collectData(runs=numRuns)

#Un-comment this for a single run (verbosity allowed). Prints (Floor Reached, Eclipse Reached and Hunts Taken)
#print(tower.beginRun())

