## Created 2/13/03 Ben Stabler benjamin.stabler@odot.state.or.us
## Need to write calculate header functions

## Copyright (C) 2002  Oregon Department of Transportation
## This program is free software; you can redistribute it and/or
## modify it under the terms of the GNU General Public License
## as published by the Free Software Foundation; either version 2
## of the License, or (at your option) any later version.

## This program is distributed in the hope that it will be useful,
## but WITHOUT ANY WARRANTY; without even the implied warranty of
## MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
## GNU General Public License for more details.

## You should have received a copy of the GNU General Public License
## along with this program; if not, write to the Free Software
## Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA

#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#Read DBF format
#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

read.dbf <- function(dbf.name) {
  infile<-file(dbf.name,"rb")
  
  ## Header
  file.version  <- readBin(infile, integer(),  1, size=1, endian="little")
  file.year     <- readBin(infile, integer(),  1, size=1, endian="little")
  file.month    <- readBin(infile, integer(),  1, size=1, endian="little")
  file.day      <- readBin(infile, integer(),  1, size=1, endian="little")
  num.records   <- readBin(infile, integer(),  1, size=4, endian="little")
  header.length <- readBin(infile, integer(),  1, size=2, endian="little")
  record.length <- readBin(infile, integer(),  1, size=2, endian="little")
  file.temp     <- readBin(infile, integer(), 20, size=1, endian="little")
  header        <- list (file.version, file.year, file.month, file.day, num.records, header.length, record.length)
  names(header) <- c ("file.version","file.year","file.month","file.day","num.records","header.length","record.length")
  rm (file.version,file.year, file.month, file.day, num.records, header.length, record.length)
  
  ## Calculate the number of fields
  num.fields   <- (header$header.length-32-1)/32
  field.name   <- NULL
  field.type   <- NULL
  field.length <- NULL
  
  ## Field Descriptions (32 bytes each)
  for (i in 1:num.fields) {
    field.name.test <- readBin (infile, character(), 1, size=10, endian="little")
    field.name      <- c (field.name, field.name.test)
    if (nchar (field.name.test)!=10) {
      file.temp <- readBin (infile,integer(), 10 - (nchar (field.name.test)), 1, endian="little")
    }
    ## RH 2003-02-16: replaced readBin by readChar in next line 
    field.type   <- c (field.type, readChar (infile, 1))  
    ## RH 2003-02-16: incremented by 1 to 4 items in next line 
    ## to compensate for above change 
    file.temp    <- readBin (infile, integer(),  4, 1, endian="little")  
    field.length <- c (field.length, readBin (infile, integer(), 1, 1, endian="little")) 
    file.temp    <- readBin (infile, integer(), 15, 1, endian="little")
  }
  
  ## Create a table of the field info
  fields <- data.frame (NAME=field.name, TYPE=field.type, LENGTH=field.length)
  ## Set all fields with length<0 equal to correct number of characters
  fields$LENGTH [fields$LENGTH<0] <- (256 + fields$LENGTH [fields$LENGTH<0])
  ## Read in end of attribute descriptions terminator - should be integer value 13
  file.temp <- readBin(infile,integer(), 1, 1, endian="little")
  ## Increase the length of field 1 by one to account for the space at the beginning of each record
  fields$LENGTH[1] <- fields$LENGTH[1]+1
  ## Add fields to the header list
  header <- c (header, fields=NULL)
  header$fields <- fields

  ## Read in all the records data and the end of file value - should be value 26
  all.records <- readBin(infile, integer(), header$num.records*header$record.length, size=1, endian="little")
  file.temp <- readBin(infile,integer(), 1, 1, endian="little")
  close(infile)
  
  ## Compress the binary values using run length encoding
  all.records <- rle(all.records)
  ## Swap ASCII decimal codes for ASCII character codes
  ascii <- c(32,46,48,49,50,51,52,53,54,55,56,57,65,66,67,68,69,70,71,72,73,74,75,76,
             77,78,79,80,81,82,83,84,85,86,87,88,89,90,97,98,99,100,101,102,103,104,
             105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,
             33,35,36,37,38,39,40,41,42,43,44,45,47,58,59,60,61,62,63,64,91,92,93,94,
             95,123,124,125,126)
  ascii.values <- c(" ",".","0","1","2","3","4","5","6","7","8","9","A","B","C","D","E","F",
                    "G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X",
                    "Y","Z","a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p",
                    "q","r","s","t","u","v","w","x","y","z","!","#","$","%","&","'","(",")",
                    "*","+","\\,","-","/",":",";","<","=",">","?","@","["," ","]","^","_",
                    "{","|","}","~")
  all.records$values <- ascii.values [match (as.character (all.records$values), as.character(ascii), nomatch=1)]
  all.records <- inverse.rle (all.records)

  ## Create a matrix of the ASCII data by record
  base.data <- t (matrix (all.records, header$record.length, header$num.records))
  rm (all.records)

  ## Function to collapse the ASCII codes, string split them and replace " " with ""
  format.record <- function(record) { 
    record <- paste(record,collapse="")
    record <- substring(record, c(1,cumsum(fields$LENGTH)[1:length(cumsum(fields$LENGTH))-1]+1),cumsum(fields$LENGTH))
    record <- gsub(" + ","", record)
    record
  }
  ## Format the base.data ASCII record stream
  dbf <- as.data.frame (t (apply (base.data, 1, format.record)))
  ## Set the numeric fields to numeric
  for (i in 1:ncol(dbf)) {
    ## RH 2003-02-16: added next line for date type setting, settin tz recommended for circumventing problems with difftime
    if(fields$TYPE[i]=="D") {dbf[,i] <- ISOdate (year=substr (as.character (dbf[,i]), 1, 4),
                                                 month=substr (as.character (dbf[,i]), 5, 6),
                                                 day=substr (as.character (dbf[,i]), 7, 8), tz="GMT")}
    #if(fields$TYPE[i]=="D") {dbf[,i] <- strptime (as.character (dbf[,i]), format="%Y%m%d")}
    if(fields$TYPE[i]=="C") {dbf[,i] <- as.character (dbf[,i])}
    if(fields$TYPE[i]=="N") {dbf[,i] <- as.numeric (as.character (dbf[,i]))}
    if(fields$TYPE[i]=="F") {dbf[,i] <- as.numeric (as.character (dbf[,i]))
                             warning("Possible trouble converting numeric field in the DBF\n") 
                           } 
  }  
  colnames (dbf) <- as.character(fields$NAME) 
  list (dbf=dbf, header=header)
} 

#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#Write out DBF format
#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

write.dbf <- function(dbf, out.name) {
  outfile<-file(out.name,"wb")
  ## File Header 
  writeBin(as.integer(3), outfile, 1, endian="little") 
  writeBin(as.integer(paste("1",substr(strsplit(date()," ")[[1]][5],3,5), sep="")), outfile, 1, endian="little")
  writeBin(as.integer(1), outfile, 1, endian="little")
  writeBin(as.integer(strsplit(date()," ")[[1]][3]), outfile, 1, endian="little")
  writeBin(as.integer(dbf$header$num.records), outfile, 4, endian="little")
  writeBin(as.integer(dbf$header$header.length), outfile, 2, endian="little")
  writeBin(as.integer(dbf$header$record.length), outfile, 2, endian="little")
  for (i in 1:20) { writeBin(as.integer(0), outfile, 1, endian="little") }
  
  ## Remove field length + 1 for record start value when writing out 
  dbf$header$fields[1,3] <- dbf$header$fields[1,3] - 1 
  
  ## Field Attributes (32 bytes for each field) 
  for (field in 1:ncol(dbf$dbf)) {
    ## Write field name
    field.name <- colnames(dbf$dbf)[field]
    writeBin(as.character(field.name), outfile, 10, endian="little")
    if (nchar(as.character(field.name))<10) {
      for (i in 1:(10-nchar(field.name))) { writeBin(as.integer(0), outfile, 1, endian="little") }
    }
    ## Write field type (C,N,Y,D)
    writeChar(as.character(dbf$header$fields[field,2]), outfile, 1, eos = NULL)
    for (i in 1:4) { writeBin(as.integer(0), outfile, 1, endian="little") }
    ## Write field length
    writeBin(as.integer(dbf$header$fields[field,3]), outfile, 1, endian="little")
    for (i in 1:15) { writeBin(as.integer(0), outfile, 1, endian="little") }
  }
  ## Write end of header value
  writeBin(as.integer(13), outfile, 1, endian="little")
  
  ## Convert dbf to characters
  data.as.char  <- apply(dbf$dbf,2,as.character)
  field.lengths <- dbf$header$fields$LENGTH
  
  ## Write out records
  for (i in 1:nrow(dbf$dbf)) {
    ## Write out record start value
    writeBin(as.integer(32), outfile, 1, endian="little")
    ## Calculate white space to add to fields in each record
    rep.amount <- field.lengths-nchar(data.as.char[i,])
    white.space <- sapply(rep.amount, function(x) rep(" ", x))
    ## Concatenate white space and existing field data
    white.space <- matrix(lapply(white.space, function(x) paste(x, collapse="")))
    record <- paste(data.as.char[i,],white.space, sep="", collapse="")
    ## Write out record data
    writeChar(as.character(record), outfile, nchar(record), eos = NULL)
  }
  
  ## Write end of file value
  writeBin(as.integer(26), outfile, 1, endian="little")
  close(outfile)
}

#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
