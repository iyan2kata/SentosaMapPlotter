#KILL_COLOR_BLUE = 42662
KILL_COLOR_BLUE = 43176
require "rmagick"
[16,17,18].each do |zoom_level_folder|
  puts "FOUND FOLDER #{zoom_level_folder}"
   `ls "#{zoom_level_folder}"`.split("\n").each do |coordinate_folder|
     `ls "#{zoom_level_folder}/#{coordinate_folder}"`.split("\n").each do |image|
       image_path = "#{zoom_level_folder}/#{coordinate_folder}/#{image}"
       image_dominant_color = Magick::Image.read(image_path).first.scale(1, 1).pixel_color(0,0).blue
       if(image_dominant_color == KILL_COLOR_BLUE)
         puts "IMAGE SCHEDULED FOR BLUERMINATION"
         `rm -rf #{image_path}`
       end       
     end
   end
 end