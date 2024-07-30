/*     Unity GIS Tech 2020-2021      */
using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class DbfColumn : ICloneable
    {
         /// </summary>
        public enum DbfColumnType
        {

            /// <summary>
            /// C Character   All OEM code page characters - padded with blanks to the width of the field.
            /// Character  less than 254 length
            /// ASCII text less than 254 characters long in dBASE. 
            /// 
            /// Character fields can be up to 32 KB long (in Clipper and FoxPro) using decimal 
            /// count as high byte in field length. It's possible to use up to 64KB long fields 
            /// by reading length as unsigned.
            /// 
            /// </summary>
            Character = 0,

            /// <summary>
            /// Number 	Length: less than 18 
            ///   ASCII text up till 18 characters long (include sign and decimal point). 
            /// 
            /// Valid characters: 
            ///    "0" - "9" and "-". Number fields can be up to 20 characters long in FoxPro and Clipper. 
            /// </summary>
            /// <remarks>
            /// We are not enforcing this 18 char limit.
            /// </remarks>
            Number = 1,

            /// <summary>
            ///  L  Logical  Length: 1    Boolean/byte (8 bit) 
            ///  
            ///  Legal values: 
            ///   ? 	Not initialised (default)
            ///   Y,y 	Yes
            ///   N,n 	No
            ///   F,f 	False
            ///   T,t 	True
            ///   Logical fields are always displayed using T/F/?. Some sources claims 
            ///   that space (ASCII 20h) is valid for not initialised. Space may occur, but is not defined. 	 
            /// </summary>
            Boolean = 2,

            /// <summary>
            /// D 	Date 	Length: 8  Date in format YYYYMMDD. A date like 0000-00- 00 is *NOT* valid. 
            /// </summary>
            Date = 3,

            /// <summary>
            /// M 	Memo 	Length: 10 	Pointer to ASCII text field in memo file 10 digits representing a pointer to a DBT block (default is blanks). 
            /// </summary>
            Memo = 4,

            /// <summary>
            /// B 	Binary 	 	(dBASE V) Like Memo fields, but not for text processing.
            /// </summary>
            Binary = 5,

            /// <summary>
            /// I 	Integer 	Length: 4 byte little endian integer 	(FoxPro)
            /// </summary>
            Integer = 6,

            /// <summary>
            /// F	Float	Number stored as a string, right justified, and padded with blanks to the width of the field. 
            /// example: 
            /// value = " 2.40000000000e+001" Length=19  Decimal_Count=11
            /// 
            /// This type was added in DBF V4.
            /// </summary>
            Float = 7,

            /// <summary>
            /// O       Double         8 bytes - no conversions, stored as a double.
            /// </summary>
            //Double = 8


        }


        /// <summary>
        /// Column (field) name
        /// </summary>
        private string _name;


        /// <summary>
        /// Field Type (Char, number, boolean, date, memo, binary)
        /// </summary>
        private DbfColumnType _type;


        /// <summary>
        /// Offset from the start of the record
        /// </summary>
        internal int _dataAddress;


        /// <summary>
        /// Length of the data in bytes; some rules apply which are in the spec (read more above).
        /// </summary>
        private int _length;


        /// <summary>
        /// Decimal precision count, or number of digits afer decimal point. This applies to Number types only.
        /// </summary>
        private int _decimalCount;



        /// <summary>
        /// Full spec constructor sets all relevant fields.
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="type"></param>
        /// <param name="nLength"></param>
        /// <param name="nDecimals"></param>
        public DbfColumn(string sName, DbfColumnType type, int nLength, int nDecimals)
        {

            Name = sName;
            _type = type;
            _length = nLength;

            if (type == DbfColumnType.Number || type == DbfColumnType.Float)
                _decimalCount = nDecimals;
            else
                _decimalCount = 0;



            //perform some simple integrity checks...
            //-------------------------------------------

            //decimal precision:
            //we could also fix the length property with a statement like this: mLength = mDecimalCount + 2;
            if (_decimalCount > 0 && _length - _decimalCount <= 1)
                throw new Exception("Decimal precision can not be larger than the length of the field.");

            if (_type == DbfColumnType.Integer)
                _length = 4;

            if (_type == DbfColumnType.Binary)
                _length = 1;

            if (_type == DbfColumnType.Date)
                _length = 8;  //Dates are exactly yyyyMMdd

            if (_type == DbfColumnType.Memo)
                _length = 10;  //Length: 10 Pointer to ASCII text field in memo file. pointer to a DBT block.

            if (_type == DbfColumnType.Boolean)
                _length = 1;

            //field length:
            if (_length <= 0)
                throw new Exception("Invalid field length specified. Field length can not be zero or less than zero.");
            else if (type != DbfColumnType.Character && type != DbfColumnType.Binary && _length > 255)
                throw new Exception("Invalid field length specified. For numbers it should be within 20 digits, but we allow up to 255. For Char and binary types, length up to 65,535 is allowed. For maximum compatibility use up to 255.");
            else if ((type == DbfColumnType.Character || type == DbfColumnType.Binary) && _length > 65535)
                throw new Exception("Invalid field length specified. For Char and binary types, length up to 65535 is supported. For maximum compatibility use up to 255.");


        }


        /// <summary>
        /// Create a new column fully specifying all properties.
        /// </summary>
        /// <param name="sName">column name</param>
        /// <param name="type">type of field</param>
        /// <param name="nLength">field length including decimal places and decimal point if any</param>
        /// <param name="nDecimals">decimal places</param>
        /// <param name="nDataAddress">offset from start of record</param>
        internal DbfColumn(string sName, DbfColumnType type, int nLength, int nDecimals, int nDataAddress)
            : this(sName, type, nLength, nDecimals)
        {

            _dataAddress = nDataAddress;

        }


        public DbfColumn(string sName, DbfColumnType type)
            : this(sName, type, 0, 0)
        {
            if (type == DbfColumnType.Number || type == DbfColumnType.Float || type == DbfColumnType.Character)
                throw new Exception("For number and character field types you must specify Length and Decimal Precision.");

        }


        /// <summary>
        /// Field Name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                //name:
                if (string.IsNullOrEmpty(value))
                    throw new Exception("Field names must be at least one char long and can not be null.");

                if (value.Length > 11)
                    throw new Exception("Field names can not be longer than 11 chars.");

                _name = value;

            }

        }


        /// <summary>
        /// Field Type (C N L D or M).
        /// </summary>
        public DbfColumnType ColumnType
        {
            get
            {
                return _type;
            }
        }


        /// <summary>
        /// Returns column type as a char, (as written in the DBF column header)
        /// N=number, C=char, B=binary, L=boolean, D=date, I=integer, M=memo
        /// </summary>
        public char ColumnTypeChar
        {
            get
            {
                switch (_type)
                {
                    case DbfColumnType.Number:
                        return 'N';

                    case DbfColumnType.Character:
                        return 'C';

                    case DbfColumnType.Binary:
                        return 'B';

                    case DbfColumnType.Boolean:
                        return 'L';

                    case DbfColumnType.Date:
                        return 'D';

                    case DbfColumnType.Integer:
                        return 'I';

                    case DbfColumnType.Memo:
                        return 'M';

                    case DbfColumnType.Float:
                        return 'F';
                }

                throw new Exception("Unrecognized field type!");

            }
        }


        /// <summary>
        /// Field Data Address offset from the start of the record.
        /// </summary>
        public int DataAddress
        {
            get
            {
                return _dataAddress;
            }
        }

        /// <summary>
        /// Length of the data in bytes.
        /// </summary>
        public int Length
        {
            get
            {
                return _length;
            }
        }

        /// <summary>
        /// Field decimal count in Binary, indicating where the decimal is.
        /// </summary>
        public int DecimalCount
        {
            get
            {
                return _decimalCount;
            }

        }





        /// <summary>
        /// Returns corresponding dbf field type given a .net Type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DbfColumnType GetDbaseType(Type type)
        {

            if (type == typeof(string))
                return DbfColumnType.Character;
            else if (type == typeof(double) || type == typeof(float))
                return DbfColumnType.Number;
            else if (type == typeof(bool))
                return DbfColumnType.Boolean;
            else if (type == typeof(DateTime))
                return DbfColumnType.Date;

            throw new NotSupportedException(String.Format("{0} does not have a corresponding dbase type.", type.Name));

        }

        public static DbfColumnType GetDbaseType(char c)
        {
            switch (c.ToString().ToUpper())
            {
                case "C": return DbfColumnType.Character;
                case "N": return DbfColumnType.Number;
                case "B": return DbfColumnType.Binary;
                case "L": return DbfColumnType.Boolean;
                case "D": return DbfColumnType.Date;
                case "I": return DbfColumnType.Integer;
                case "M": return DbfColumnType.Memo;
                case "F": return DbfColumnType.Float;
            }

            throw new NotSupportedException(String.Format("{0} does not have a corresponding dbase type.", c));

        }

        /// <summary>
        /// Returns shp file Shape Field.
        /// </summary>
        /// <returns></returns>
        public static DbfColumn ShapeField()
        {
            return new DbfColumn("GISTerrainLoaderGeometryGen", DbfColumnType.Binary);

        }


        /// <summary>
        /// Returns Shp file ID field.
        /// </summary>
        /// <returns></returns>
        public static DbfColumn IdField()
        {
            return new DbfColumn("Row", DbfColumnType.Integer);

        }



        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class DbfDataTruncateException : Exception
    {

        public DbfDataTruncateException(string smessage) : base(smessage)
        {
        }

        public DbfDataTruncateException(string smessage, Exception innerException)
          : base(smessage, innerException)
        {
        }

        public DbfDataTruncateException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }

    }
    public class DbfFile
    {

        /// <summary>
        /// Helps read/write dbf file header information.
        /// </summary>
        protected DbfHeader _header;


        /// <summary>
        /// flag that indicates whether the header was written or not...
        /// </summary>
        protected bool _headerWritten = false;


        /// <summary>
        /// Streams to read and write to the DBF file.
        /// </summary>
        protected Stream _dbfFile = null;
        protected BinaryReader _dbfFileReader = null;
        protected BinaryWriter _dbfFileWriter = null;

        /// <summary>
        /// By default use windows 1252 code page encoding.
        /// </summary>
        private Encoding encoding = Encoding.GetEncoding(1252);

        /// <summary>
        /// File that was opened, if one was opened at all.
        /// </summary>
        protected string _fileName = "";


        /// <summary>
        /// Number of records read using ReadNext() methods only. This applies only when we are using a forward-only stream.
        /// mRecordsReadCount is used to keep track of record index. With a seek enabled stream, 
        /// we can always calculate index using stream position.
        /// </summary>
        public long _recordsReadCount = 0;


        /// <summary>
        /// keep these values handy so we don't call functions on every read.
        /// </summary>
        protected bool _isForwardOnly = false;
        protected bool _isReadOnly = false;


        //[Obsolete("Method is obsolete.", false)]
        public DbfFile()
            : this(Encoding.GetEncoding(1252))
        {
        }

        public DbfFile(Encoding encoding)
        {
            this.encoding = encoding;
            _header = new DbfHeader(encoding);
        }

        /// <summary>
        /// Open a DBF from a FileStream. This can be a file or an internet connection stream. Make sure that it is positioned at start of DBF file.
        /// Reading a DBF over the internet we can not determine size of the file, so we support HasMore(), ReadNext() interface. 
        /// RecordCount information in header can not be trusted always, since some packages store 0 there.
        /// </summary>
        /// <param name="ofs"></param>
        public void Open(Stream ofs)
        {
            if (_dbfFile != null)
                Close();

            _dbfFile = ofs;
            _dbfFileReader = null;
            _dbfFileWriter = null;

            if (_dbfFile.CanRead)
                _dbfFileReader = new BinaryReader(_dbfFile, encoding);

            if (_dbfFile.CanWrite)
                _dbfFileWriter = new BinaryWriter(_dbfFile, encoding);

            //reset position
            _recordsReadCount = 0;

            //assume header is not written
            _headerWritten = false;

            //read the header
            if (ofs.CanRead)
            {
                //try to read the header...
                try
                {
                    _header.Read(_dbfFileReader);
                    _headerWritten = true;

                }
                catch (EndOfStreamException)
                {
                    //could not read header, file is empty
                    _header = new DbfHeader(encoding);
                    _headerWritten = false;
                }


            }

            if (_dbfFile != null)
            {
                _isReadOnly = !_dbfFile.CanWrite;
                _isForwardOnly = !_dbfFile.CanSeek;
            }


        }



        /// <summary>
        /// Open a DBF file or create a new one.
        /// </summary>
        /// <param name="sPath">Full path to the file.</param>
        /// <param name="mode"></param>
        public void Open(string sPath, FileMode mode, FileAccess access, FileShare share)
        {
            _fileName = sPath;
            Open(File.Open(sPath, mode, access, share));
        }

        /// <summary>
        /// Open a DBF file or create a new one.
        /// </summary>
        /// <param name="sPath">Full path to the file.</param>
        /// <param name="mode"></param>
        public void Open(string sPath, FileMode mode, FileAccess access)
        {
            _fileName = sPath;
            Open(File.Open(sPath, mode, access));
        }

        /// <summary>
        /// Open a DBF file or create a new one.
        /// </summary>
        /// <param name="sPath">Full path to the file.</param>
        /// <param name="mode"></param>
        public void Open(string sPath, FileMode mode)
        {
            _fileName = sPath;
            Open(File.Open(sPath, mode));
        }


        /// <summary>
        /// Creates a new DBF 4 file. Overwrites if file exists! Use Open() function for more options.
        /// </summary>
        /// <param name="sPath"></param>
        public void Create(string sPath)
        {
            Open(sPath, FileMode.Create, FileAccess.ReadWrite);
            _headerWritten = false;

        }



        /// <summary>
        /// Update header info, flush buffers and close streams. You should always call this method when you are done with a DBF file.
        /// </summary>
        public void Close()
        {

            //try to update the header if it has changed
            //------------------------------------------
            if (_header.IsDirty)
                WriteHeader();



            //Empty header...
            //--------------------------------
            _header = new DbfHeader(encoding);
            _headerWritten = false;


            //reset current record index
            //--------------------------------
            _recordsReadCount = 0;


            //Close streams...
            //--------------------------------
            if (_dbfFileWriter != null)
            {
                _dbfFileWriter.Flush();
                _dbfFileWriter.Close();
            }

            if (_dbfFileReader != null)
                _dbfFileReader.Close();

            if (_dbfFile != null)
            {
                _dbfFile.Close();
                _dbfFile.Dispose();
            }


            //set streams to null
            //--------------------------------
            _dbfFileReader = null;
            _dbfFileWriter = null;
            _dbfFile = null;

            _fileName = "";

        }



        /// <summary>
        /// Returns true if we can not write to the DBF file stream.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return _isReadOnly;
                /*
                if (mDbfFile != null)
                  return !mDbfFile.CanWrite; 
                return true;
                */

            }

        }


        /// <summary>
        /// Returns true if we can not seek to different locations within the file, such as internet connections.
        /// </summary>
        public bool IsForwardOnly
        {
            get
            {
                return _isForwardOnly;
                /*
                if(mDbfFile!=null)
                  return !mDbfFile.CanSeek;
        
                return false;
                */
            }
        }


        /// <summary>
        /// Returns the name of the filestream.
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }



        /// <summary>
        /// Read next record and fill data into parameter oFillRecord. Returns true if a record was read, otherwise false.
        /// </summary>
        /// <param name="oFillRecord"></param>
        /// <returns></returns>
        public bool ReadNext(DbfRecord oFillRecord)
        {

            //check if we can fill this record with data. it must match record size specified by header and number of columns.
            //we are not checking whether it comes from another DBF file or not, we just need the same structure. Allow flexibility but be safe.
            if (oFillRecord.Header != _header && (oFillRecord.Header.ColumnCount != _header.ColumnCount || oFillRecord.Header.RecordLength != _header.RecordLength))
                throw new Exception("Record parameter does not have the same size and number of columns as the " +
                                    "header specifies, so we are unable to read a record into oFillRecord. " +
                                    "This is a programming error, have you mixed up DBF file objects?");

            //DBF file reader can be null if stream is not readable...
            if (_dbfFileReader == null)
                throw new Exception("Read stream is null, either you have opened a stream that can not be " +
                                    "read from (a write-only stream) or you have not opened a stream at all.");

            //read next record...
            bool bRead = oFillRecord.Read(_dbfFile);

            if (bRead)
            {
                if (_isForwardOnly)
                {
                    //zero based index! set before incrementing count.
                    oFillRecord.RecordIndex = _recordsReadCount;
                    _recordsReadCount++;
                }
                else
                    oFillRecord.RecordIndex = ((int)((_dbfFile.Position - _header.HeaderLength) / _header.RecordLength)) - 1;

            }

            return bRead;

        }


        /// <summary>
        /// Tries to read a record and returns a new record object or null if nothing was read.
        /// </summary>
        /// <returns></returns>
        public DbfRecord ReadNext()
        {
            //create a new record and fill it.
            DbfRecord orec = new DbfRecord(_header);

            return ReadNext(orec) ? orec : null;

        }



        /// <summary>
        /// Reads a record specified by index into oFillRecord object. You can use this method 
        /// to read in and process records without creating and discarding record objects.
        /// Note that you should check that your stream is not forward-only! If you have a forward only stream, use ReadNext() functions.
        /// </summary>
        /// <param name="index">Zero based record index.</param>
        /// <param name="oFillRecord">Record object to fill, must have same size and number of fields as thid DBF file header!</param>
        /// <remarks>
        /// <returns>True if read a record was read, otherwise false. If you read end of file false will be returned and oFillRecord will NOT be modified!</returns>
        /// The parameter record (oFillRecord) must match record size specified by the header and number of columns as well.
        /// It does not have to come from the same header, but it must match the structure. We are not going as far as to check size of each field.
        /// The idea is to be flexible but safe. It's a fine balance, these two are almost always at odds.
        /// </remarks>
        public bool Read(long index, DbfRecord oFillRecord)
        {

            //check if we can fill this record with data. it must match record size specified by header and number of columns.
            //we are not checking whether it comes from another DBF file or not, we just need the same structure. Allow flexibility but be safe.
            if (oFillRecord.Header != _header && (oFillRecord.Header.ColumnCount != _header.ColumnCount || oFillRecord.Header.RecordLength != _header.RecordLength))
                throw new Exception("Record parameter does not have the same size and number of columns as the " +
                                    "header specifies, so we are unable to read a record into oFillRecord. " +
                                    "This is a programming error, have you mixed up DBF file objects?");

            //DBF file reader can be null if stream is not readable...
            if (_dbfFileReader == null)
                throw new Exception("ReadStream is null, either you have opened a stream that can not be " +
                                    "read from (a write-only stream) or you have not opened a stream at all.");


            //move to the specified record, note that an exception will be thrown is stream is not seekable! 
            //This is ok, since we provide a function to check whether the stream is seekable. 
            long nSeekToPosition = _header.HeaderLength + (index * _header.RecordLength);

            //check whether requested record exists. Subtract 1 from file length (there is a terminating character 1A at the end of the file)
            //so if we hit end of file, there are no more records, so return false;
            if (index < 0 || _dbfFile.Length - 1 <= nSeekToPosition)
                return false;

            //move to record and read
            _dbfFile.Seek(nSeekToPosition, SeekOrigin.Begin);

            //read the record
            bool bRead = oFillRecord.Read(_dbfFile);
            if (bRead)
                oFillRecord.RecordIndex = index;

            return bRead;

        }

        public bool ReadValue(int rowIndex, int columnIndex, out string result)
        {

            result = String.Empty;

            DbfColumn ocol = _header[columnIndex];

            //move to the specified record, note that an exception will be thrown is stream is not seekable! 
            //This is ok, since we provide a function to check whether the stream is seekable. 
            long nSeekToPosition = _header.HeaderLength + (rowIndex * _header.RecordLength) + ocol.DataAddress;

            //check whether requested record exists. Subtract 1 from file length (there is a terminating character 1A at the end of the file)
            //so if we hit end of file, there are no more records, so return false;
            if (rowIndex < 0 || _dbfFile.Length - 1 <= nSeekToPosition)
                return false;

            //move to position and read
            _dbfFile.Seek(nSeekToPosition, SeekOrigin.Begin);

            //read the value
            byte[] data = new byte[ocol.Length];
            _dbfFile.Read(data, 0, ocol.Length);
            result = new string(encoding.GetChars(data, 0, ocol.Length));

            return true;
        }

        /// <summary>
        /// Reads a record specified by index. This method requires the stream to be able to seek to position. 
        /// If you are using a http stream, or a stream that can not stream, use ReadNext() methods to read in all records.
        /// </summary>
        /// <param name="index">Zero based index.</param>
        /// <returns>Null if record can not be read, otherwise returns a new record.</returns>
        public DbfRecord Read(long index)
        {
            //create a new record and fill it.
            DbfRecord orec = new DbfRecord(_header);

            return Read(index, orec) ? orec : null;

        }




        /// <summary>
        /// Write a record to file. If RecordIndex is present, record will be updated, otherwise a new record will be written.
        /// Header will be output first if this is the first record being writen to file. 
        /// This method does not require stream seek capability to add a new record.
        /// </summary>
        /// <param name="orec"></param>
        public void Write(DbfRecord orec)
        {

            //if header was never written, write it first, then output the record
            if (!_headerWritten)
                WriteHeader();

            //if this is a new record (RecordIndex should be -1 in that case)
            if (orec.RecordIndex < 0)
            {
                if (_dbfFileWriter.BaseStream.CanSeek)
                {
                    //calculate number of records in file. do not rely on header's RecordCount property since client can change that value.
                    //also note that some DBF files do not have ending 0x1A byte, so we subtract 1 and round off 
                    //instead of just cast since cast would just drop decimals.
                    int nNumRecords = (int)Math.Round(((double)(_dbfFile.Length - _header.HeaderLength - 1) / _header.RecordLength));
                    if (nNumRecords < 0)
                        nNumRecords = 0;

                    orec.RecordIndex = nNumRecords;
                    Update(orec);
                    _header.RecordCount++;

                }
                else
                {
                    //we can not position this stream, just write out the new record.
                    orec.Write(_dbfFile);
                    _header.RecordCount++;
                }
            }
            else
                Update(orec);

        }

        public void Write(DbfRecord orec, bool bClearRecordAfterWrite)
        {

            Write(orec);

            if (bClearRecordAfterWrite)
                orec.Clear();

        }


        /// <summary>
        /// Update a record. RecordIndex (zero based index) must be more than -1, otherwise an exception is thrown.
        /// You can also use Write method which updates a record if it has RecordIndex or adds a new one if RecordIndex == -1.
        /// RecordIndex is set automatically when you call any Read() methods on this class.
        /// </summary>
        /// <param name="orec"></param>
        public void Update(DbfRecord orec)
        {

            //if header was never written, write it first, then output the record
            if (!_headerWritten)
                WriteHeader();


            //Check if record has an index
            if (orec.RecordIndex < 0)
                throw new Exception("RecordIndex is not set, unable to update record. Set RecordIndex or call Write() method to add a new record to file.");


            //Check if this record matches record size specified by header and number of columns. 
            //Client can pass a record from another DBF that is incompatible with this one and that would corrupt the file.
            if (orec.Header != _header && (orec.Header.ColumnCount != _header.ColumnCount || orec.Header.RecordLength != _header.RecordLength))
                throw new Exception("Record parameter does not have the same size and number of columns as the " +
                                    "header specifies. Writing this record would corrupt the DBF file. " +
                                    "This is a programming error, have you mixed up DBF file objects?");

            //DBF file writer can be null if stream is not writable to...
            if (_dbfFileWriter == null)
                throw new Exception("Write stream is null. Either you have opened a stream that can not be " +
                                    "writen to (a read-only stream) or you have not opened a stream at all.");


            //move to the specified record, note that an exception will be thrown if stream is not seekable! 
            //This is ok, since we provide a function to check whether the stream is seekable. 
            long nSeekToPosition = (long)_header.HeaderLength + (long)((long)orec.RecordIndex * (long)_header.RecordLength);

            //check whether we can seek to this position. Subtract 1 from file length (there is a terminating character 1A at the end of the file)
            //so if we hit end of file, there are no more records, so return false;
            if (_dbfFile.Length < nSeekToPosition)
                throw new Exception("Invalid record position. Unable to save record.");

            //move to record start
            _dbfFile.Seek(nSeekToPosition, SeekOrigin.Begin);

            //write
            orec.Write(_dbfFile);


        }



        /// <summary>
        /// Save header to file. Normally, you do not have to call this method, header is saved 
        /// automatically and updated when you close the file (if it changed).
        /// </summary>
        public bool WriteHeader()
        {

            //update header if possible
            //--------------------------------
            if (_dbfFileWriter != null)
            {
                if (_dbfFileWriter.BaseStream.CanSeek)
                {
                    _dbfFileWriter.Seek(0, SeekOrigin.Begin);
                    _header.Write(_dbfFileWriter);
                    _headerWritten = true;
                    return true;
                }
                else
                {
                    //if stream can not seek, then just write it out and that's it.
                    if (!_headerWritten)
                        _header.Write(_dbfFileWriter);

                    _headerWritten = true;

                }
            }

            return false;

        }



        /// <summary>
        /// Access DBF header with information on columns. Use this object for faster access to header. 
        /// Remove one layer of function calls by saving header reference and using it directly to access columns.
        /// </summary>
        public DbfHeader Header
        {
            get
            {
                return _header;
            }
        }



    }
    public class DbfHeader : ICloneable
    {

        /// <summary>
        /// Header file descriptor size is 33 bytes (32 bytes + 1 terminator byte), followed by column metadata which is 32 bytes each.
        /// </summary>
        public const int FileDescriptorSize = 33;


        /// <summary>
        /// Field or DBF Column descriptor is 32 bytes long.
        /// </summary>
        public const int ColumnDescriptorSize = 32;


        //type of the file, must be 03h
        private const int _fileType = 0x03;

        //Date the file was last updated.
        private DateTime _updateDate;

        //Number of records in the datafile, 32bit little-endian, unsigned 
        private uint _numRecords = 0;

        //Length of the header structure
        private ushort _headerLength = FileDescriptorSize;  //empty header is 33 bytes long. Each column adds 32 bytes.

        //Length of the records, ushort - unsigned 16 bit integer
        private int _recordLength = 1;  //start with 1 because the first byte is a delete flag

        //DBF fields/columns
        internal List<DbfColumn> _fields = new List<DbfColumn>();


        //indicates whether header columns can be modified!
        bool _locked = false;

        //keeps column name index for the header, must clear when header columns change.
        private Dictionary<string, int> _columnNameIndex = null;

        /// <summary>
        /// When object is modified dirty flag is set.
        /// </summary>
        bool _isDirty = false;


        /// <summary>
        /// mEmptyRecord is an array used to clear record data in CDbf4Record.
        /// This is shared by all record objects, used to speed up clearing fields or entire record.
        /// <seealso cref="EmptyDataRecord"/>
        /// </summary>
        private byte[] _emptyRecord = null;


        public readonly Encoding encoding = Encoding.ASCII;


        [Obsolete]
        public DbfHeader()
        {
        }

        public DbfHeader(Encoding encoding)
        {
            this.encoding = encoding;
        }


        /// <summary>
        /// Specify initial column capacity.
        /// </summary>
        /// <param name="nInitialFields"></param>
        public DbfHeader(int nFieldCapacity)
        {
            _fields = new List<DbfColumn>(nFieldCapacity);

        }


        /// <summary>
        /// Gets header length.
        /// </summary>
        public ushort HeaderLength
        {
            get
            {
                return _headerLength;
            }
        }


        /// <summary>
        /// Add a new column to the DBF header.
        /// </summary>
        /// <param name="oNewCol"></param>
        public void AddColumn(DbfColumn oNewCol)
        {

            //throw exception if the header is locked
            if (_locked)
                throw new InvalidOperationException("This header is locked and can not be modified. Modifying the header would result in a corrupt DBF file. You can unlock the header by calling UnLock() method.");

            //since we are breaking the spec rules about max number of fields, we should at least 
            //check that the record length stays within a number that can be recorded in the header!
            //we have 2 unsigned bytes for record length for a maximum of 65535.
            if (_recordLength + oNewCol.Length > 65535)
                throw new ArgumentOutOfRangeException("oNewCol", "Unable to add new column. Adding this column puts the record length over the maximum (which is 65535 bytes).");


            //add the column
            _fields.Add(oNewCol);

            //update offset bits, record and header lengths
            oNewCol._dataAddress = _recordLength;
            _recordLength += oNewCol.Length;
            _headerLength += ColumnDescriptorSize;

            //clear empty record
            _emptyRecord = null;

            //set dirty bit
            _isDirty = true;
            _columnNameIndex = null;

        }


        /// <summary>
        /// Create and add a new column with specified name and type.
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="type"></param>
        public void AddColumn(string sName, DbfColumn.DbfColumnType type)
        {
            AddColumn(new DbfColumn(sName, type));
        }


        /// <summary>
        /// Create and add a new column with specified name, type, length, and decimal precision.
        /// </summary>
        /// <param name="sName">Field name. Uniqueness is not enforced.</param>
        /// <param name="type"></param>
        /// <param name="nLength">Length of the field including decimal point and decimal numbers</param>
        /// <param name="nDecimals">Number of decimal places to keep.</param>
        public void AddColumn(string sName, DbfColumn.DbfColumnType type, int nLength, int nDecimals)
        {
            AddColumn(new DbfColumn(sName, type, nLength, nDecimals));
        }


        /// <summary>
        /// Remove column from header definition.
        /// </summary>
        /// <param name="nIndex"></param>
        public void RemoveColumn(int nIndex)
        {
            //throw exception if the header is locked
            if (_locked)
                throw new InvalidOperationException("This header is locked and can not be modified. Modifying the header would result in a corrupt DBF file. You can unlock the header by calling UnLock() method.");


            DbfColumn oColRemove = _fields[nIndex];
            _fields.RemoveAt(nIndex);


            oColRemove._dataAddress = 0;
            _recordLength -= oColRemove.Length;
            _headerLength -= ColumnDescriptorSize;

            //if you remove a column offset shift for each of the columns 
            //following the one removed, we need to update those offsets.
            int nRemovedColLen = oColRemove.Length;
            for (int i = nIndex; i < _fields.Count; i++)
                _fields[i]._dataAddress -= nRemovedColLen;

            //clear the empty record
            _emptyRecord = null;

            //set dirty bit
            _isDirty = true;
            _columnNameIndex = null;

        }


        /// <summary>
        /// Look up a column index by name. NOT Case Sensitive. This is a change from previous behaviour!
        /// </summary>
        /// <param name="sName"></param>
        public DbfColumn this[string sName]
        {
            get
            {
                int colIndex = FindColumn(sName);
                if (colIndex > -1)
                    return _fields[colIndex];

                return null;

            }
        }


        /// <summary>
        /// Returns column at specified index. Index is 0 based.
        /// </summary>
        /// <param name="nIndex">Zero based index.</param>
        /// <returns></returns>
        public DbfColumn this[int nIndex]
        {
            get
            {
                return _fields[nIndex];
            }
        }


        /// <summary>
        /// Finds a column index by using a fast dictionary lookup-- creates column dictionary on first use. Returns -1 if not found. CHANGE: not case sensitive any longer!
        /// </summary>
        /// <param name="sName">Column name (case insensitive comparison)</param>
        /// <returns>column index (0 based) or -1 if not found.</returns>
        public int FindColumn(string sName)
        {

            if (_columnNameIndex == null)
            {
                _columnNameIndex = new Dictionary<string, int>(_fields.Count);

                //create a new index
                for (int i = 0; i < _fields.Count; i++)
                {
                    _columnNameIndex.Add(_fields[i].Name.ToUpper(), i);
                }
            }

            int columnIndex;
            if (_columnNameIndex.TryGetValue(sName.ToUpper(), out columnIndex))
                return columnIndex;

            return -1;

        }


        /// <summary>
        /// Returns an empty data record. This is used to clear columns 
        /// </summary>
        /// <remarks>
        /// The reason we put this in the header class is because it allows us to use the CDbf4Record class in two ways.
        /// 1. we can create one instance of the record and reuse it to write many records quickly clearing the data array by bitblting to it.
        /// 2. we can create many instances of the record (a collection of records) and have only one copy of this empty dataset for all of them.
        ///    If we had put it in the Record class then we would be taking up twice as much space unnecessarily. The empty record also fits the model
        ///    and everything is neatly encapsulated and safe.
        /// 
        /// </remarks>
        protected internal byte[] EmptyDataRecord
        {
            get { return _emptyRecord ?? (_emptyRecord = encoding.GetBytes("".PadLeft(_recordLength, ' ').ToCharArray())); }
        }


        /// <summary>
        /// Returns Number of columns in this dbf header.
        /// </summary>
        public int ColumnCount
        {
            get { return _fields.Count; }

        }


        /// <summary>
        /// Size of one record in bytes. All fields + 1 byte delete flag.
        /// </summary>
        public int RecordLength
        {
            get
            {
                return _recordLength;
            }
        }


        /// <summary>
        /// Get/Set number of records in the DBF.
        /// </summary>
        /// <remarks>
        /// The reason we allow client to set RecordCount is beause in certain streams 
        /// like internet streams we can not update record count as we write out records, we have to set it in advance,
        /// so client has to be able to modify this property.
        /// </remarks>
        public uint RecordCount
        {
            get
            {
                return _numRecords;
            }

            set
            {
                _numRecords = value;

                //set the dirty bit
                _isDirty = true;

            }
        }


        /// <summary>
        /// Get/set whether this header is read only or can be modified. When you create a CDbfRecord 
        /// object and pass a header to it, CDbfRecord locks the header so that it can not be modified any longer.
        /// in order to preserve DBF integrity.
        /// </summary>
        internal bool Locked
        {
            get
            {
                return _locked;
            }

            set
            {
                _locked = value;
            }

        }


        /// <summary>
        /// Use this method with caution. Headers are locked for a reason, to prevent DBF from becoming corrupt.
        /// </summary>
        public void Unlock()
        {
            _locked = false;
        }


        /// <summary>
        /// Returns true when this object is modified after read or write.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }

            set
            {
                _isDirty = value;
            }
        }


        /// <summary>
        /// Encoding must be ASCII for this binary writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <remarks>
        /// See class remarks for DBF file structure.
        /// </remarks>
        public void Write(BinaryWriter writer)
        {

            //write the header
            // write the output file type.
            writer.Write((byte)_fileType);

            //Update date format is YYMMDD, which is different from the column Date type (YYYYDDMM)
            writer.Write((byte)(_updateDate.Year - 1900));
            writer.Write((byte)_updateDate.Month);
            writer.Write((byte)_updateDate.Day);

            // write the number of records in the datafile. (32 bit number, little-endian unsigned)
            writer.Write(_numRecords);

            // write the length of the header structure.
            writer.Write(_headerLength);

            // write the length of a record
            writer.Write((ushort)_recordLength);

            // write the reserved bytes in the header
            for (int i = 0; i < 20; i++)
                writer.Write((byte)0);

            // write all of the header records
            byte[] byteReserved = new byte[14];  //these are initialized to 0 by default.
            foreach (DbfColumn field in _fields)
            {
                char[] cname = field.Name.PadRight(11, (char)0).ToCharArray();
                writer.Write(cname);

                // write the field type
                writer.Write((char)field.ColumnTypeChar);

                // write the field data address, offset from the start of the record.
                writer.Write(field.DataAddress);


                // write the length of the field.
                // if char field is longer than 255 bytes, then we use the decimal field as part of the field length.
                if (field.ColumnType == DbfColumn.DbfColumnType.Character && field.Length > 255)
                {
                    //treat decimal count as high byte of field length, this extends char field max to 65535
                    writer.Write((ushort)field.Length);

                }
                else
                {
                    // write the length of the field.
                    writer.Write((byte)field.Length);

                    // write the decimal count.
                    writer.Write((byte)field.DecimalCount);
                }

                // write the reserved bytes.
                writer.Write(byteReserved);

            }

            // write the end of the field definitions marker
            writer.Write((byte)0x0D);
            writer.Flush();

            //clear dirty bit
            _isDirty = false;


            //lock the header so it can not be modified any longer, 
            //we could actually postpond this until first record is written!
            _locked = true;


        }


        /// <summary>
        /// Read header data, make sure the stream is positioned at the start of the file to read the header otherwise you will get an exception.
        /// When this function is done the position will be the first record.
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {

            // type of reader.
            int nFileType = reader.ReadByte();

            if (nFileType != 0x03)
                throw new NotSupportedException("Unsupported DBF reader Type " + nFileType);

            // parse the update date information.
            int year = (int)reader.ReadByte();
            int month = (int)reader.ReadByte();
            int day = (int)reader.ReadByte();
            _updateDate = new DateTime(year + 1900, month, day);

            // read the number of records.
            _numRecords = reader.ReadUInt32();

            // read the length of the header structure.
            _headerLength = reader.ReadUInt16();

            // read the length of a record
            _recordLength = reader.ReadInt16();

            // skip the reserved bytes in the header.
            reader.ReadBytes(20);

            // calculate the number of Fields in the header
            int nNumFields = (_headerLength - FileDescriptorSize) / ColumnDescriptorSize;

            //offset from start of record, start at 1 because that's the delete flag.
            int nDataOffset = 1;

            // read all of the header records
            _fields = new List<DbfColumn>(nNumFields);
            for (int i = 0; i < nNumFields; i++)
            {

                // read the field name				
                char[] buffer = new char[11];
                buffer = reader.ReadChars(11);
                string sFieldName = new string(buffer);
                int nullPoint = sFieldName.IndexOf((char)0);
                if (nullPoint != -1)
                    sFieldName = sFieldName.Substring(0, nullPoint);


                //read the field type
                char cDbaseType = (char)reader.ReadByte();

                // read the field data address, offset from the start of the record.
                int nFieldDataAddress = reader.ReadInt32();


                //read the field length in bytes
                //if field type is char, then read FieldLength and Decimal count as one number to allow char fields to be
                //longer than 256 bytes (ASCII char). This is the way Clipper and FoxPro do it, and there is really no downside
                //since for char fields decimal count should be zero for other versions that do not support this extended functionality.
                //-----------------------------------------------------------------------------------------------------------------------
                int nFieldLength = 0;
                int nDecimals = 0;
                if (cDbaseType == 'C' || cDbaseType == 'c')
                {
                    //treat decimal count as high byte
                    nFieldLength = (int)reader.ReadUInt16();
                }
                else
                {
                    //read field length as an unsigned byte.
                    nFieldLength = (int)reader.ReadByte();

                    //read decimal count as one byte
                    nDecimals = (int)reader.ReadByte();

                }


                //read the reserved bytes.
                reader.ReadBytes(14);

                //Create and add field to collection
                _fields.Add(new DbfColumn(sFieldName, DbfColumn.GetDbaseType(cDbaseType), nFieldLength, nDecimals, nDataOffset));

                // add up address information, you can not trust the address recorded in the DBF file...
                nDataOffset += nFieldLength;

            }

            // Last byte is a marker for the end of the field definitions.
            reader.ReadBytes(1);


            //read any extra header bytes...move to first record
            //equivalent to reader.BaseStream.Seek(mHeaderLength, SeekOrigin.Begin) except that we are not using the seek function since
            //we need to support streams that can not seek like web connections.
            int nExtraReadBytes = _headerLength - (FileDescriptorSize + (ColumnDescriptorSize * _fields.Count));
            if (nExtraReadBytes > 0)
                reader.ReadBytes(nExtraReadBytes);



            //if the stream is not forward-only, calculate number of records using file size, 
            //sometimes the header does not contain the correct record count
            //if we are reading the file from the web, we have to use ReadNext() functions anyway so
            //Number of records is not so important and we can trust the DBF to have it stored correctly.
            if (reader.BaseStream.CanSeek && _numRecords == 0)
            {
                //notice here that we subtract file end byte which is supposed to be 0x1A,
                //but some DBF files are incorrectly written without this byte, so we round off to nearest integer.
                //that gives a correct result with or without ending byte.
                if (_recordLength > 0)
                    _numRecords = (uint)Math.Round(((double)(reader.BaseStream.Length - _headerLength - 1) / _recordLength));

            }


            //lock header since it was read from a file. we don't want it modified because that would corrupt the file.
            //user can override this lock if really necessary by calling UnLock() method.
            _locked = true;

            //clear dirty bit
            _isDirty = false;

        }



        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class DbfReader : IDisposable
    {
        private readonly DbfFile dbfFile;

        public DbfReader()
        {
            dbfFile = new DbfFile();
        }

        public DbfReader(string filepath)
            : this()
        {
            this.Open(filepath);
        }

        public DbfReader(Stream stream)
            : this()
        {
            this.Open(stream);
        }

        public void Open(string filepath)
        {
            dbfFile.Open(filepath, FileMode.Open);
        }

        public void Open(Stream stream)
        {
            dbfFile.Open(stream);
        }

        public void Close()
        {
            dbfFile.Close();
        }

        public void Dispose()
        {
            this.Close();
        }

        public DbfRecord CreateRecord()
        {
            return new DbfRecord(dbfFile.Header);
        }

        public bool ReadNext(DbfRecord record)
        {
            return dbfFile.ReadNext(record);
        }
    }
    public class DbfRecord
    {

        /// <summary>
        /// Header provides information on all field types, sizes, precision and other useful information about the DBF.
        /// </summary>
        private DbfHeader _header = null;

        /// <summary>
        /// Dbf data are a mix of ASCII characters and binary, which neatly fit in a byte array.
        /// BinaryWriter would esentially perform the same conversion using the same Encoding class.
        /// </summary>
        private byte[] _data = null;

        /// <summary>
        /// Zero based record index. -1 when not set, new records for example.
        /// </summary>
        private long _recordIndex = -1;

        /// <summary>
        /// Empty Record array reference used to clear fields quickly (or entire record).
        /// </summary>
        private readonly byte[] _emptyRecord = null;


        /// <summary>
        /// Specifies whether we allow strings to be truncated. If false and string is longer than we can fit in the field, an exception is thrown.
        /// </summary>
        private bool _allowStringTruncate = true;

        /// <summary>
        /// Specifies whether we allow the decimal portion of numbers to be truncated. 
        /// If false and decimal digits overflow the field, an exception is thrown.
        /// </summary>
        private bool _allowDecimalTruncate = false;

        /// <summary>
        /// Specifies whether we allow the integer portion of numbers to be truncated.
        /// If false and integer digits overflow the field, an exception is thrown.
        /// </summary>
        private bool _allowIntegerTruncate = false;


        //array used to clear decimals, we can clear up to 40 decimals which is much more than is allowed under DBF spec anyway.
        //Note: 48 is ASCII code for 0.
        private static readonly byte[] _decimalClear = new byte[] {48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,
                                                               48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,
                                                               48,48,48,48,48,48,48,48,48,48,48,48,48,48,48};


        //Warning: do not make this one static because that would not be thread safe!! The reason I have 
        //placed this here is to skip small memory allocation/deallocation which fragments memory in .net.
        private int[] _tempIntVal = { 0 };


        //encoder
        private readonly Encoding encoding = Encoding.GetEncoding(1252);

        /// <summary>
        /// Column Name to Column Index map
        /// </summary>
        private readonly Dictionary<string, int> _colNameToIdx = new Dictionary<string, int>(StringComparer.InvariantCulture);



        /// <summary>
        /// 
        /// </summary>
        /// <param name="oHeader">Dbf Header will be locked once a record is created 
        /// since the record size is fixed and if the header was modified it would corrupt the DBF file.</param>
        public DbfRecord(DbfHeader oHeader)
        {
            _header = oHeader;
            _header.Locked = true;

            //create a buffer to hold all record data. We will reuse this buffer to write all data to the file.
            _data = new byte[_header.RecordLength];

            // Make sure mData[0] correctly represents 'not deleted'
            IsDeleted = false;

            _emptyRecord = _header.EmptyDataRecord;
            encoding = oHeader.encoding;

            for (int i = 0; i < oHeader._fields.Count; i++)
                _colNameToIdx[oHeader._fields[i].Name] = i;
        }


        /// <summary>
        /// Set string data to a column, if the string is longer than specified column length it will be truncated!
        /// If dbf column type is not a string, input will be treated as dbf column 
        /// type and if longer than length an exception will be thrown.
        /// </summary>
        /// <param name="nColIndex"></param>
        /// <returns></returns>
        public string this[int nColIndex]
        {

            set
            {

                DbfColumn ocol = _header[nColIndex];
                DbfColumn.DbfColumnType ocolType = ocol.ColumnType;


                //
                //if an empty value is passed, we just clear the data, and leave it blank.
                //note: test have shown that testing for null and checking length is faster than comparing to "" empty str :)
                //------------------------------------------------------------------------------------------------------------
                if (string.IsNullOrEmpty(value))
                {
                    //this is like NULL data, set it to empty. i looked at SAS DBF output when a null value exists 
                    //and empty data are output. we get the same result, so this looks good.
                    Buffer.BlockCopy(_emptyRecord, ocol.DataAddress, _data, ocol.DataAddress, ocol.Length);

                }
                else
                {

                    //set values according to data type:
                    //-------------------------------------------------------------
                    if (ocolType == DbfColumn.DbfColumnType.Character)
                    {
                        if (!_allowStringTruncate && value.Length > ocol.Length)
                            throw new DbfDataTruncateException("Value not set. String truncation would occur and AllowStringTruncate flag is set to false. To supress this exception change AllowStringTruncate to true.");

                        //BlockCopy copies bytes.  First clear the previous value, then set the new one.
                        Buffer.BlockCopy(_emptyRecord, ocol.DataAddress, _data, ocol.DataAddress, ocol.Length);
                        encoding.GetBytes(value, 0, value.Length > ocol.Length ? ocol.Length : value.Length, _data, ocol.DataAddress);

                    }
                    else if (ocolType == DbfColumn.DbfColumnType.Number)
                    {

                        if (ocol.DecimalCount == 0)
                        {

                            //integers
                            //----------------------------------

                            //throw an exception if integer overflow would occur
                            if (!_allowIntegerTruncate && value.Length > ocol.Length)
                                throw new DbfDataTruncateException("Value not set. Integer does not fit and would be truncated. AllowIntegerTruncate is set to false. To supress this exception set AllowIntegerTruncate to true, although that is not recomended.");


                            //clear all numbers, set to [space].
                            //-----------------------------------------------------
                            Buffer.BlockCopy(_emptyRecord, 0, _data, ocol.DataAddress, ocol.Length);


                            //set integer part, CAREFUL not to overflow buffer! (truncate instead)
                            //-----------------------------------------------------------------------
                            int nNumLen = value.Length > ocol.Length ? ocol.Length : value.Length;
                            encoding.GetBytes(value, 0, nNumLen, _data, (ocol.DataAddress + ocol.Length - nNumLen));

                        }
                        else
                        {

                            ///TODO: we can improve perfomance here by not using temp char arrays cDec and cNum,
                            ///simply directly copy from source string using encoding!


                            //break value down into integer and decimal portions
                            //--------------------------------------------------------------------------
                            int nidxDecimal = value.IndexOf('.'); //index where the decimal point occurs
                            char[] cDec = null; //decimal portion of the number
                            char[] cNum = null; //integer portion

                            if (nidxDecimal > -1)
                            {
                                cDec = value.Substring(nidxDecimal + 1).Trim().ToCharArray();
                                cNum = value.Substring(0, nidxDecimal).ToCharArray();

                                //throw an exception if decimal overflow would occur
                                if (!_allowDecimalTruncate && cDec.Length > ocol.DecimalCount)
                                    throw new DbfDataTruncateException("Value not set. Decimal does not fit and would be truncated. AllowDecimalTruncate is set to false. To supress this exception set AllowDecimalTruncate to true.");

                            }
                            else
                                cNum = value.ToCharArray();


                            //throw an exception if integer overflow would occur
                            if (!_allowIntegerTruncate && cNum.Length > ocol.Length - ocol.DecimalCount - 1)
                                throw new DbfDataTruncateException("Value not set. Integer does not fit and would be truncated. AllowIntegerTruncate is set to false. To supress this exception set AllowIntegerTruncate to true, although that is not recomended.");


                            //------------------------------------------------------------------------------------------------------------------
                            // NUMERIC TYPE
                            //------------------------------------------------------------------------------------------------------------------

                            //clear all decimals, set to 0.
                            //-----------------------------------------------------
                            Buffer.BlockCopy(_decimalClear, 0, _data, (ocol.DataAddress + ocol.Length - ocol.DecimalCount), ocol.DecimalCount);

                            //clear all numbers, set to [space].
                            Buffer.BlockCopy(_emptyRecord, 0, _data, ocol.DataAddress, (ocol.Length - ocol.DecimalCount));



                            //set decimal numbers, CAREFUL not to overflow buffer! (truncate instead)
                            //-----------------------------------------------------------------------
                            if (nidxDecimal > -1)
                            {
                                int nLen = cDec.Length > ocol.DecimalCount ? ocol.DecimalCount : cDec.Length;
                                encoding.GetBytes(cDec, 0, nLen, _data, (ocol.DataAddress + ocol.Length - ocol.DecimalCount));
                            }

                            //set integer part, CAREFUL not to overflow buffer! (truncate instead)
                            //-----------------------------------------------------------------------
                            int nNumLen = cNum.Length > ocol.Length - ocol.DecimalCount - 1 ? (ocol.Length - ocol.DecimalCount - 1) : cNum.Length;
                            encoding.GetBytes(cNum, 0, nNumLen, _data, ocol.DataAddress + ocol.Length - ocol.DecimalCount - nNumLen - 1);


                            //set decimal point
                            //-----------------------------------------------------------------------
                            _data[ocol.DataAddress + ocol.Length - ocol.DecimalCount - 1] = (byte)'.';


                        }


                    }
                    else if (ocolType == DbfColumn.DbfColumnType.Float)
                    {
                        //------------------------------------------------------------------------------------------------------------------
                        // FLOAT TYPE
                        // example:   value=" 2.40000000000e+001"  Length=19   Decimal-Count=11
                        //------------------------------------------------------------------------------------------------------------------


                        // check size, throw exception if value won't fit:
                        if (value.Length > ocol.Length)
                            throw new DbfDataTruncateException("Value not set. Float value does not fit and would be truncated.");


                        double parsed_value;
                        if (!Double.TryParse(value, out parsed_value))
                        {
                            //value did not parse, input is not correct.
                            throw new DbfDataTruncateException("Value not set. Float value format is bad: '" + value + "'   expected format: ' 2.40000000000e+001'");
                        }

                        //clear value that was present previously
                        Buffer.BlockCopy(_decimalClear, 0, _data, ocol.DataAddress, ocol.Length);

                        //copy new value at location
                        char[] valueAsCharArray = value.ToCharArray();
                        encoding.GetBytes(valueAsCharArray, 0, valueAsCharArray.Length, _data, ocol.DataAddress);

                    }
                    else if (ocolType == DbfColumn.DbfColumnType.Integer)
                    {
                        //note this is a binary Integer type!
                        //----------------------------------------------

                        ///TODO: maybe there is a better way to copy 4 bytes from int to byte array. Some memory function or something.
                        _tempIntVal[0] = Convert.ToInt32(value);
                        Buffer.BlockCopy(_tempIntVal, 0, _data, ocol.DataAddress, 4);

                    }
                    else if (ocolType == DbfColumn.DbfColumnType.Memo)
                    {
                        //copy 10 digits...
                        ///TODO: implement MEMO

                        throw new NotImplementedException("Memo data type functionality not implemented yet!");

                    }
                    else if (ocolType == DbfColumn.DbfColumnType.Boolean)
                    {
                        if (String.Compare(value, "true", true) == 0 || String.Compare(value, "1", true) == 0 ||
                            String.Compare(value, "T", true) == 0 || String.Compare(value, "yes", true) == 0 ||
                            String.Compare(value, "Y", true) == 0)
                            _data[ocol.DataAddress] = (byte)'T';
                        else if (value == " " || value == "?")
                            _data[ocol.DataAddress] = (byte)'?';
                        else
                            _data[ocol.DataAddress] = (byte)'F';

                    }
                    else if (ocolType == DbfColumn.DbfColumnType.Date)
                    {
                        //try to parse out date value using Date.Parse() function, then set the value
                        DateTime dateval;
                        if (DateTime.TryParse(value, out dateval))
                        {
                            SetDateValue(nColIndex, dateval);
                        }
                        else
                            throw new InvalidOperationException("Date could not be parsed from source string! Please parse the Date and set the value (you can try using DateTime.Parse() or DateTime.TryParse() functions).");

                    }
                    else if (ocolType == DbfColumn.DbfColumnType.Binary)
                        throw new InvalidOperationException("Can not use string source to set binary data. Use SetBinaryValue() and GetBinaryValue() functions instead.");

                    else
                        throw new InvalidDataException("Unrecognized data type: " + ocolType.ToString());

                }

            }

            get
            {
                DbfColumn ocol = _header[nColIndex];
                return new string(encoding.GetChars(_data, ocol.DataAddress, ocol.Length));

            }
        }

        /// <summary>
        /// Set string data to a column, if the string is longer than specified column length it will be truncated!
        /// If dbf column type is not a string, input will be treated as dbf column 
        /// type and if longer than length an exception will be thrown.
        /// </summary>
        /// <param name="nColName"></param>
        /// <returns></returns>
        public string this[string nColName]
        {
            get
            {
                if (_colNameToIdx.ContainsKey(nColName))
                    return this[_colNameToIdx[nColName]];
                throw new InvalidOperationException(string.Format("There's no column with name '{0}'", nColName));
            }
            set
            {
                if (_colNameToIdx.ContainsKey(nColName))
                    this[_colNameToIdx[nColName]] = value;
                else
                    throw new InvalidOperationException(string.Format("There's no column with name '{0}'", nColName));
            }
        }

        /// <summary>
        /// Get date value.
        /// </summary>
        /// <param name="nColIndex"></param>
        /// <returns></returns>
        public DateTime GetDateValue(int nColIndex)
        {
            DbfColumn ocol = _header[nColIndex];

            if (ocol.ColumnType == DbfColumn.DbfColumnType.Date)
            {
                string sDateVal = encoding.GetString(_data, ocol.DataAddress, ocol.Length);
                return DateTime.ParseExact(sDateVal, "yyyyMMdd", CultureInfo.InvariantCulture);

            }
            else
                throw new Exception("Invalid data type. Column '" + ocol.Name + "' is not a date column.");

        }


        /// <summary>
        /// Get date value.
        /// </summary>
        /// <param name="nColIndex"></param>
        /// <returns></returns>
        public void SetDateValue(int nColIndex, DateTime value)
        {

            DbfColumn ocol = _header[nColIndex];
            DbfColumn.DbfColumnType ocolType = ocol.ColumnType;


            if (ocolType == DbfColumn.DbfColumnType.Date)
            {

                //Format date and set value, date format is like this: yyyyMMdd
                //-------------------------------------------------------------
                encoding.GetBytes(value.ToString("yyyyMMdd"), 0, ocol.Length, _data, ocol.DataAddress);

            }
            else
                throw new Exception("Invalid data type. Column is of '" + ocol.ColumnType.ToString() + "' type, not date.");


        }


        /// <summary>
        /// Clears all data in the record.
        /// </summary>
        public void Clear()
        {
            Buffer.BlockCopy(_emptyRecord, 0, _data, 0, _emptyRecord.Length);
            _recordIndex = -1;

        }


        /// <summary>
        /// returns a string representation of this record.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new string(encoding.GetChars(_data));
        }


        /// <summary>
        /// Gets/sets a zero based record index. This information is not directly stored in DBF. 
        /// It is the location of this record within the DBF. 
        /// </summary>
        /// <remarks>
        /// This property is managed from outside this object,
        /// CDbfFile object updates it when records are read. The reason we don't set it in the Read() 
        /// function within this object is that the stream can be forward-only so the Position property 
        /// is not available and there is no way to figure out what index the record was unless you 
        /// count how many records were read, and that's exactly what CDbfFile does.
        /// </remarks>
        public long RecordIndex
        {
            get
            {
                return _recordIndex;
            }
            set
            {
                _recordIndex = value;
            }
        }


        /// <summary>
        /// Returns/sets flag indicating whether this record was tagged deleted. 
        /// </summary>
        /// <remarks>Use CDbf4File.Compress() function to rewrite dbf removing records flagged as deleted.</remarks>
        /// <seealso cref="CDbf4File.Compress() function"/>
        public bool IsDeleted
        {
            get { return _data[0] == '*'; }
            set { _data[0] = value ? (byte)'*' : (byte)' '; }
        }


        /// <summary>
        /// Specifies whether strings can be truncated. If false and string is longer than can fit in the field, an exception is thrown.
        /// Default is True.
        /// </summary>
        public bool AllowStringTurncate
        {
            get { return _allowStringTruncate; }
            set { _allowStringTruncate = value; }
        }

        /// <summary>
        /// Specifies whether to allow the decimal portion of numbers to be truncated. 
        /// If false and decimal digits overflow the field, an exception is thrown. Default is false.
        /// </summary>
        public bool AllowDecimalTruncate
        {
            get { return _allowDecimalTruncate; }
            set { _allowDecimalTruncate = value; }
        }


        /// <summary>
        /// Specifies whether integer portion of numbers can be truncated.
        /// If false and integer digits overflow the field, an exception is thrown. 
        /// Default is False.
        /// </summary>
        public bool AllowIntegerTruncate
        {
            get { return _allowIntegerTruncate; }
            set { _allowIntegerTruncate = value; }
        }


        /// <summary>
        /// Returns header object associated with this record.
        /// </summary>
        public DbfHeader Header
        {
            get
            {
                return _header;
            }
        }


        /// <summary>
        /// Get column by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DbfColumn Column(int index)
        {
            return _header[index];
        }

        /// <summary>
        /// Get column by name.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DbfColumn Column(string sName)
        {
            return _header[sName];
        }

        /// <summary>
        /// Gets column count from header.
        /// </summary>
        public int ColumnCount
        {
            get
            {
                return _header.ColumnCount;
            }
        }

        /// <summary>
        /// Finds a column index by searching sequentially through the list. Case is ignored. Returns -1 if not found.
        /// </summary>
        /// <param name="sName">Column name.</param>
        /// <returns>Column index (0 based) or -1 if not found.</returns>
        public int FindColumn(string sName)
        {
            return _header.FindColumn(sName);
        }

        /// <summary>
        /// Writes data to stream. Make sure stream is positioned correctly because we simply write out the data to it.
        /// </summary>
        /// <param name="osw"></param>
        protected internal void Write(Stream osw)
        {
            osw.Write(_data, 0, _data.Length);

        }


        /// <summary>
        /// Writes data to stream. Make sure stream is positioned correctly because we simply write out data to it, and clear the record.
        /// </summary>
        /// <param name="osw"></param>
        protected internal void Write(Stream obw, bool bClearRecordAfterWrite)
        {
            obw.Write(_data, 0, _data.Length);

            if (bClearRecordAfterWrite)
                Clear();

        }


        /// <summary>
        /// Read record from stream. Returns true if record read completely, otherwise returns false.
        /// </summary>
        /// <param name="obr"></param>
        /// <returns></returns>
        protected internal bool Read(Stream obr)
        {
            return obr.Read(_data, 0, _data.Length) >= _data.Length;
        }

        protected internal string ReadValue(Stream obr, int colIndex)
        {
            DbfColumn ocol = _header[colIndex];
            return new string(encoding.GetChars(_data, ocol.DataAddress, ocol.Length));

        }

    }
}
