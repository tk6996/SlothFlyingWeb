# Sloth Flying Website

เริ่มแรกในการเริ่มต้น Project ใช้คำสั่ง
```shell
$ dotnet restore
```

เสร็จแล้วสามารถ Run เพื่อใช้งาน
```shell
$ dotnet watch run
```
Path ของ web application
```shell
wwwroot/
├─ Admin/
│  ├─ Blacklist
│  ├─ Login
│  └─ Logout
├─ DinoLab/
│  ├─ Index
│  └─ Lab{labId}
├─ Home/
│  ├─ Error
│  └─ Index
├─ Lab/
│  ├─ Booking{labId}
│  └─ Index
├─ LabAdmin/
│  ├─ EditItem{labId}
│  ├─ Index
│  └─ ViewItem{labId}
├─ Search/
│  ├─ Index
│  ├─ UserBooklist{userId}
│  └─ UserProfile{userId}
├─ User/
│  ├─ Booklist
│  ├─ EditProfile
│  ├─ Login
│  ├─ Logout
│  ├─ Profile
│  └─ Register
└─ Index
```
Path ของ api
```shell
wwwroot/
└─ Api/
   └─ GetBooking/{labId} # Get จำนวนของที่สามารถยืมได้ที่เหลือในแต่ละช่วงเวลา
```
p.s. กรณีที่ pull มาใหม่ควรลบ database ที่มีอยู่เดิมไปก่อนเพราะ schema อาจจะเปลี่ยน
