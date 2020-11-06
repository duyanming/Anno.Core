namespace java com.javabloger.gen.code   #  注释1

service Ilb {  #  注释3 
    bool remove_broker(1:map<string,string>  input)
	bool add_broker(1:map<string,string>  input)
	bool save_broker()
}