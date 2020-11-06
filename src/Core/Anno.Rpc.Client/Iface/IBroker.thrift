namespace csharp Anno.Rpc   #  注释1
struct Micro {   #  注释2
    1: string ip
    2: i32 port
    3: i32 timeout
	4: string name
    5: string nickname
	6: i32 weight
  }
service BrokerService {  #  注释3
    string broker(1:map<string,string>  input)
}
service BrokerCenter {  #  注释3
	bool add_broker(1:map<string,string>  input)
	list<Micro> GetMicro(1:string  channel)
    string Invoke(1:map<string,string>  input)
}
